using Microsoft.EntityFrameworkCore;
using PlayerLeaderboardApi.Context;
using PlayerLeaderboardApi.Context.Entities;

namespace PlayerLeaderboardApi.Endpoints;

public record AchievementDto(string Name, int Points);
public record GetAchievementDto(int Id, string Name, string DisplayName, int Points);

public static class AchievementsEndpoints
{
    public static void MapAchievementsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public/achievements");

        group.MapGet("/top", async (AppDbContext db, int limit = 300) =>
        {
            var achievements = await db.Achievements
                .Take(limit)
                .Select(a => new { a.DisplayName, a.Points })
                .OrderByDescending(a => a.Points)
                .ToListAsync();

            return Results.Ok(achievements);
        });

        group.MapGet("/id/{id}", async (int id, AppDbContext db) =>
        {
            var achievement = await db.Achievements.FirstOrDefaultAsync(a => a.Id == id);

            if (achievement == null) return Results.NotFound();
            return Results.Ok(achievement);
        });

        group.MapGet("/{name}", async (string name, AppDbContext db) =>
        {
            var achievement = await db.Achievements.FirstOrDefaultAsync(a => a.Name == name);

            if (achievement == null) return Results.NotFound();
            return Results.Ok(achievement);
        });

        group.MapGet("/{name}/id", async (string name, AppDbContext db) =>
        {
            var achievementId = await db.Achievements
                .Where(a => a.Name == name)
                .Select(a => a.Id)
                .FirstOrDefaultAsync();

            if (achievementId == default) return Results.NotFound();
            return Results.Ok(achievementId);
        });

        app.MapPost("/api/achievements", async (AppDbContext db, AchievementDto dto) =>
        {
            bool isNameEmpty = string.IsNullOrEmpty(dto.Name);
            bool isNameInvalid = dto.Name.All(c => char.IsControl(c));
            bool isLengthInvalid = dto.Name.Length > 25 || dto.Name.Length < 3;
            if (isNameEmpty || isNameInvalid || isLengthInvalid) return Results.BadRequest("Invalid name");

            bool nameExists = await db.Achievements.AnyAsync(p => p.Name.Equals(dto.Name.ToLower()));
            if (nameExists) return Results.Conflict($"An achievement with the name '{dto.Name}' already exists");

            bool isPointsInvalid = dto.Points < 0 || dto.Points > 999999999;
            if (isPointsInvalid) return Results.BadRequest("Invalid points");

            var achievement = new Achievement
            {
                Name = dto.Name.ToLower(),
                DisplayName = dto.Name,
                Points = dto.Points
            };

            db.Achievements.Add(achievement);
            await db.SaveChangesAsync();
            
            return Results.Ok(achievement);
        });

        app.MapDelete("/api/achievements/id/{id:int}", async (AppDbContext db, int id) =>
        {
            var achievement = await db.Achievements.FirstOrDefaultAsync(a => a.Id == id);
            
            if (achievement == null) return Results.NotFound();

            db.Achievements.Remove(achievement);
            await db.SaveChangesAsync();
            
            return Results.NoContent();
        });

        app.MapDelete("/api/achievements/{name}", async (AppDbContext db, string name) =>
        {
            var achievement = await db.Achievements.FirstOrDefaultAsync(a => a.Name == name);
            
            if (achievement == null) return Results.NotFound();

            db.Achievements.Remove(achievement);
            await db.SaveChangesAsync();
            
            return Results.NoContent();
        });
    }
}