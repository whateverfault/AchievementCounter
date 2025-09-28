using Newtonsoft.Json;

namespace AchievementCounter;

public class SaveData {
    [JsonProperty("api_key")]
    public string ApiKey { get; set; }
    
    [JsonProperty("steam_id")]
    public string SteamId { get; set; }
    
    [JsonProperty("app_id")]
    public string AppId { get; set; }
    
    [JsonProperty("message")]
    public string? Message { get; set; }
    
    [JsonProperty("layout")]
    public int Layout { get; set; }
    
    [JsonProperty("cooldown")]
    public int Cooldown { get; set; }
    
    [JsonProperty("debug")]
    public bool Debug { get; set; }


    public SaveData() {
        ApiKey = string.Empty;
        SteamId = string.Empty;
        AppId = string.Empty;
        Message = string.Empty;
        Layout = 1;
        Cooldown = 5000;
        Debug = false;
    }
    
    [JsonConstructor]
    public SaveData(
        [JsonProperty("api_key")] string apiKey,
        [JsonProperty("steam_id")] string steamId,
        [JsonProperty("app_id")] string appId,
        [JsonProperty("message")] string? message,
        [JsonProperty("layout")] int layout,
        [JsonProperty("cooldown")] int cooldown,
        [JsonProperty("debug")] bool debug) {
        ApiKey = apiKey;
        SteamId = steamId;
        AppId = appId;
        Message = message;
        Layout = layout;
        Cooldown = cooldown;
        Debug = debug;
    }
}