using Newtonsoft.Json;

namespace AchievementCounter.Response;

public class PlayerStats {
    [JsonProperty("achievements")]
    public Achievement[] Achievements { get; private set; }
    
    
    public PlayerStats(
        [JsonProperty("achievements")] Achievement[] achievements) {
        Achievements = achievements;
    }
}