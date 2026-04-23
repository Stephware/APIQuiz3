namespace APIQuiz3.Utils
{
    public class RefreshTokenService
    {
        private static Dictionary<string, string> RefreshTokens = new();
        private static Dictionary<string, string> UserTokens = new();

        public string GenerateRefreshToken(string username)
        {
            var token = Guid.NewGuid().ToString();

            if (UserTokens.ContainsKey(username))
            {
                var oldToken = UserTokens[username];
                RefreshTokens.Remove(oldToken);
            }

            RefreshTokens[token] = username;
            UserTokens[username] = token;

            return token;
        }

        public string Validate(string token)
        {
            return RefreshTokens.ContainsKey(token)
                ? RefreshTokens[token]
                : null;
        }

        public bool IsLatestToken(string username, string token)
        {
            return UserTokens.ContainsKey(username)
                && UserTokens[username] == token;
        }

        public void Remove(string token)
        {
            if (RefreshTokens.ContainsKey(token))
            {
                var username = RefreshTokens[token];
                RefreshTokens.Remove(token);

                if (UserTokens.ContainsKey(username))
                    UserTokens.Remove(username);
            }
        }
    }
}