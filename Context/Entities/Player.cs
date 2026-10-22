namespace PlayerLeaderboardApi.Context.Entities;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public List<Achievement> Achievements { get; } = [];
    public List<int> GetAchievementIds()
    {
        var result = Achievements.Select(a => a.Id);
        return [.. result];
    }
    public int Score => Achievements.Sum(a => a.Points);
}