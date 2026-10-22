using Microsoft.EntityFrameworkCore;
using PlayerLeaderboardApi.Context;
using PlayerLeaderboardApi.Endpoints;
using PlayerLeaderboardApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapPlayersEndpoints();
app.MapAchievementsEndpoints();
app.MapPlayerAchievementsEndpoints();

app.Run();