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
            var playerId = await db.Players
                .Select(p => p.Id)
                .FirstOrDefaultAsync(id => id == dto.PlayerId);

            var achievementId = await db.Achievements
                .Select(a => a.Id)
                .FirstOrDefaultAsync(id => id == dto.AchievementId);

            if (playerId == default  || achievementId == default) return Results.BadRequest("A player or an achievement with this id does not exist");

            bool playerAchievementExists = await db.PlayerAchievements.AnyAsync(pa => pa.PlayerId == dto.PlayerId && pa.AchievementId == dto.AchievementId);
            if (playerAchievementExists) return Results.Conflict("This entry already exists");

            var playerAchievement = new PlayerAchievement
            {
                PlayerId = dto.PlayerId,
                AchievementId = dto.AchievementId
            };

            db.PlayerAchievements.Add(playerAchievement);
            await db.SaveChangesAsync();
            
            return Results.Ok(playerAchievement);
        });

        app.MapDelete("/api/player_achievements/{playerId:int}_{achievementId:int}", async (int playerId, int achievementId, AppDbContext db) =>
        {
            var playerAchievement = await db.PlayerAchievements
                .FirstOrDefaultAsync(pa => pa.PlayerId == playerId && pa.AchievementId == achievementId);

            if (playerAchievement == null) return Results.NotFound();

            db.PlayerAchievements.Remove(playerAchievement);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}