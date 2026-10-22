using Microsoft.EntityFrameworkCore;
using PlayerLeaderboardApi.Context.Entities;

namespace PlayerLeaderboardApi.Context;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerAchievement> PlayerAchievements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>()
            .HasIndex(p => p.Name)
            .IsUnique();

        modelBuilder.Entity<Achievement>()
            .HasIndex(a => a.Name)
            .IsUnique();

        modelBuilder.Entity<Achievement>().HasData(
            new Achievement
            {
                Id = 1,
                Name = "mutant",
                DisplayName = "Mutant",
                Points = 1000
            },
            new Achievement
            {
                Id = 2,
                Name = "abominationn",
                DisplayName = "Abominationn",
                Points = 700
            },
            new Achievement
            {
                Id = 3,
                Name = "golem",
                DisplayName = "Golem",
                Points = 1000
            },
            new Achievement
            {
                Id = 4,
                Name = "moon lord",
                DisplayName = "Moon Lord",
                Points = 800
            }
        );

        modelBuilder.Entity<Player>()
            .HasMany(p => p.Achievements)
            .WithMany(a => a.Players)
            .UsingEntity<PlayerAchievement>();
    }
}
