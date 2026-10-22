using Microsoft.EntityFrameworkCore;
using PlayerLeaderboardApi.Context;
using PlayerLeaderboardApi.Context.Entities;

namespace PlayerLeaderboardApi.Endpoints;

public record AchievementDto(string Name, int Points);

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
            var achievement = await db.Achievements
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();

            return Results.Ok(achievement);
        });

        group.MapGet("/{name}", async (string name, AppDbContext db) =>
        {
            var achievement = await db.Achievements
                .Where(a => a.Name == name)
                .FirstOrDefaultAsync();

            return Results.Ok(achievement);
        });

        app.MapPost("/api/achievements", async (AppDbContext db, AchievementDto dto) =>
        {
            bool isNameEmpty = string.IsNullOrEmpty(dto.Name);
            bool isNameInvalid = dto.Name.All(c => char.IsControl(c));
            bool isLengthInvalid = dto.Name.Length > 25 || dto.Name.Length < 3;
            if (isNameEmpty || isNameInvalid || isLengthInvalid) return Results.BadRequest("Invalid name");

            bool nameExists = await db.Achievements.AnyAsync(p => p.Name.Equals(dto.Name.ToLower()));
            if (nameExists) return Results.Conflict($"An achievement with the name '{dto.Name}' already exists");

            bool isPointsInvalid = dto.Points < 0 || dto.Points > 1000000000;
            if (isPointsInvalid)
            {
                Console.WriteLine("Invalid");
                return Results.BadRequest("Invalid points");
            }

            var achievement = new Achievement
            {
                Name = dto.Name.ToLower(),
                DisplayName = dto.Name,
                Points = dto.Points
            };

            db.Achievements.Add(achievement);
            await db.SaveChangesAsync();
            
            return Results.Ok(new { message = "Successfully", achievement });
        });
    }
}