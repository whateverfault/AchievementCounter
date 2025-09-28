using Newtonsoft.Json;

namespace AchievementCounter.Response;

public class Achievement {
    [JsonProperty("apiname")]
    public string ApiName { get; private set; }
    
    [JsonProperty("achieved")]
    public bool Achieved { get; private set; }
    
    
    [JsonConstructor]
    public Achievement(
        [JsonProperty("apiname")] string apiName,
        [JsonProperty("achieved")] bool achieved) {
        ApiName = apiName;
        Achieved = achieved;
    }
}