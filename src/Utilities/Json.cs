using Newtonsoft.Json;

namespace AchievementCounter.Utilities;

public static class Json {
    private static readonly JsonSerializerSettings _options 
        = new JsonSerializerSettings { 
                                         NullValueHandling = NullValueHandling.Ignore, 
                                         Formatting = Formatting.Indented, 
                                     };

    public static void Write(object? obj, string fileName) { 
        var jsonString = JsonConvert.SerializeObject(obj, _options); 
        File.WriteAllText(fileName, jsonString);
    }

    public static void Read<T>(string fileName, out T? obj) {
        try {
            using var r = new StreamReader(fileName);
            obj = JsonConvert.DeserializeObject<T>(r.ReadToEnd());
        }
        catch {
            obj = default;
        }
    }
}