using APIQuiz3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class APIKeyAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<APIKeyAuthorizeAttribute>>();

        if (!context.HttpContext.Request.Headers.TryGetValue("X-API-KEY", out var extractedKey))
        {
            logger.LogWarning("Missing API Key");
            context.Result = new UnauthorizedObjectResult("API Key missing");
            return;
        }

        var keys = config
            .GetSection("Security:ApiKeys")
            .Get<List<ApiKeyConfig>>();

        var validKey = keys.FirstOrDefault(k => k.Key == extractedKey);

        if (validKey == null)
        {
            logger.LogWarning("Invalid API Key");
            context.Result = new UnauthorizedObjectResult("Invalid API Key");
            return;
        }

        if (validKey.Expires < DateTime.Now)
        {
            logger.LogWarning("Expired API Key");
            context.Result = new ObjectResult(new
            {
                error = "Forbidden",
                message = "API Key is expired already",
                status = 403
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}