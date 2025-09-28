using Newtonsoft.Json;

namespace AchievementCounter.Response;

public class GetPlayerAchievementsResponse {
    [JsonProperty("playerstats")]
    public PlayerStats PlayerStats { get; private set; }
    
    
    public GetPlayerAchievementsResponse(
        [JsonProperty("playerstats")] PlayerStats playerstats) {
        PlayerStats = playerstats;
    }
}