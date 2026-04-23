using APIQuiz3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using APIQuiz3.Utils;
using Microsoft.Extensions.Logging;

namespace APIQuiz3.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwt;
        private readonly RefreshTokenService _refresh;
        private readonly ILogger<AuthController> _logger; 

        private static List<(string Username, string Password, string Role)> users =
            new()
            {
            ("admin", "admin123", "Admin"),
            ("user", "user123", "User")
            };

        public AuthController(JwtService jwt, RefreshTokenService refresh, ILogger<AuthController> logger)
        {
            _jwt = jwt;
            _refresh = refresh;
            _logger = logger;
        }

        [HttpPost("login")]
        [APIKeyAuthorize]
        [EnableRateLimiting("LoginPolicy")]
        public IActionResult Login(LoginModel model)
        {
            var user = users.FirstOrDefault(u =>
                u.Username == model.Username && u.Password == model.Password);

            if (user == default)
            {
                _logger.LogWarning("Failed login attempt for username: {username}", model.Username); 
                return Unauthorized("Invalid credentials");
            }

            var token = _jwt.GenerateToken(user.Username, user.Role);
            var refreshToken = _refresh.GenerateRefreshToken(user.Username);

            _logger.LogInformation(
            "User is now logged in | Username: {username} | Role: {role} | JWT: {jwt} | RefreshToken: {refresh}",
            user.Username,
            user.Role,
            token,
            refreshToken);

            return Ok(new
            {
                token,
                refreshToken,
                role = user.Role
            });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh(RefreshTokenModel model)
        {
            var username = _refresh.Validate(model.RefreshToken);

            if (username == null)
                return Unauthorized();

            var role = users.First(u => u.Username == username).Role;

            var newToken = _jwt.GenerateToken(username, role);
            var newRefresh = _refresh.GenerateRefreshToken(username);

            return Ok(new { token = newToken, refreshToken = newRefresh });
        }

        [HttpPost("logout")]
        public IActionResult Logout(RefreshTokenModel model)
        {
            _refresh.Remove(model.RefreshToken);
            return Ok("Logged out");
        }
    }
}