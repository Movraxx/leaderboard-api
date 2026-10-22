namespace PlayerLeaderboardApi.Context.Entities;

public class Achievement
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public int Points { get; set; }
    public List<Player> Players { get; } = [];
}