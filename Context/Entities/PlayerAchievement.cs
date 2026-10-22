namespace PlayerLeaderboardApi.Context.Entities;

public class PlayerAchievement
{
    public int PlayerId { get; set; }
    public int AchievementId { get; set; }
    public Player Player { get; set;} = null!;
    public Achievement Achievement { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}