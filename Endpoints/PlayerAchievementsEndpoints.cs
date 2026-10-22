using Microsoft.EntityFrameworkCore;
using PlayerLeaderboardApi.Context;
using PlayerLeaderboardApi.Context.Entities;

namespace PlayerLeaderboardApi.Endpoints;

public record PlayerAchievementDto(int PlayerId, int AchievementId);
public static class PlayerAchievementsEndpoints
{
    public static void MapPlayerAchievementsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/player_achievements/", async (AppDbContext db, PlayerAchievementDto dto) =>
        {
            var player = await db.Players.FirstOrDefaultAsync(p => p.Id == dto.PlayerId);
            var achievement = await db.Achievements.FirstOrDefaultAsync(a => a.Id == dto.AchievementId);

            if (player == null || achievement == null) return Results.BadRequest("A player or an achievement with this id does not exist");

            bool playerAchievementExists = await db.PlayerAchievements.AnyAsync(pa => pa.PlayerId == dto.PlayerId && pa.AchievementId == dto.AchievementId);
            if (playerAchievementExists) return Results.Conflict("This entry already exists");

            var playerAchievement = new PlayerAchievement
            {
                PlayerId = dto.PlayerId,
                AchievementId = dto.AchievementId
            };

            db.PlayerAchievements.Add(playerAchievement);
            await db.SaveChangesAsync();
            
            return Results.Ok("Success");
        });
    }
}