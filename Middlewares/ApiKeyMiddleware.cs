using System.Net;

namespace PlayerLeaderboardApi.Middlewares;

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly RequestDelegate next = next;
    private const string ApiKeyHeader = "API-Key";
    private readonly string expectedApiKey = configuration["ApiKey"] ?? throw
        new InvalidOperationException("API key not configured in appsettings.json");

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/public"))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("API key is missing");
            return;
        }

        string apiKey = extractedApiKey.ToString();

        if (!expectedApiKey.Equals(apiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Unathorized");
            return;
        }

        await next(context);
    }
}
