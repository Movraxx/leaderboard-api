using Microsoft.EntityFrameworkCore;
using PlayerLeaderboardApi.Context;
using PlayerLeaderboardApi.Context.Entities;

namespace PlayerLeaderboardApi.Endpoints;

public record PlayerDto(string Name);
public record PlayersTopDto(string DisplayName, int Score);
public record GetPlayerDto(int Id, string Name, string DisplayName, List<int> AchievementIds);

public static class PlayersEndpoints
{
    public static void MapPlayersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public/players");

        group.MapGet("/top", async (AppDbContext db, int limit = 300) =>
        {
            var dbPlayers = await db.Players
                .Take(limit)
                .Include(p => p.Achievements)
                .ToListAsync();

            var players = new List<PlayersTopDto>();

            foreach (Player player in dbPlayers)
            {
                players.Add(new PlayersTopDto(player.DisplayName, player.Score));
            }
            var playersResult = players.OrderByDescending(p => p.Score);
            return Results.Ok(playersResult);
        });

        group.MapGet("/id/{id}", async (int id, AppDbContext db) =>
        {
            var player = await db.Players
                .Where(p => p.Id == id)
                .Include(p => p.Achievements)
                .FirstOrDefaultAsync();
            
            if (player == null) return Results.NotFound("A player with this id does not exist");

            var playerResult = new GetPlayerDto(player.Id, player.Name, player.DisplayName, player.GetAchievementIds());
            return Results.Ok(playerResult);
        });

        group.MapGet("/{name}", async (string name, AppDbContext db) =>
        {
            var player = await db.Players
                .Where(p => p.Name == name)
                .Include(p => p.Achievements)
                .FirstOrDefaultAsync();

            if (player == null) return Results.NotFound("A player with this name does not exist");

            var playerResult = new GetPlayerDto(player.Id, player.Name, player.DisplayName, player.GetAchievementIds());
            return Results.Ok(playerResult);
        });

        app.MapPost("/api/players", async (AppDbContext db, PlayerDto dto) =>
        {
            bool isNameEmpty = string.IsNullOrEmpty(dto.Name);
            bool isNameInvalid = dto.Name.All(c => char.IsControl(c));
            bool isLengthInvalid = dto.Name.Length > 25 || dto.Name.Length < 3;
            if (isNameEmpty || isNameInvalid || isLengthInvalid) return Results.BadRequest("Invalid name");

            bool nameExists = await db.Players.AnyAsync(p => p.Name.Equals(dto.Name.ToLower()));
            if (nameExists) return Results.Conflict($"A player with the name '{dto.Name}' already exists");

            var player = new Player
            {
                Name = dto.Name.ToLower(),
                DisplayName = dto.Name
            };

            db.Players.Add(player);
            await db.SaveChangesAsync();
            
            return Results.Ok("Success");
        });
    }
}