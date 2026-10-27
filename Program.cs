using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PlayerLeaderboardApi.Context;
using PlayerLeaderboardApi.Endpoints;
using PlayerLeaderboardApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext => 
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 30,
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(1)
            }
        ));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();

app.UseRateLimiter();

app.MapPlayersEndpoints();
app.MapAchievementsEndpoints();
app.MapPlayerAchievementsEndpoints();

app.Run();