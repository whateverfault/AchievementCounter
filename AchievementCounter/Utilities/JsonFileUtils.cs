using Newtonsoft.Json;

namespace AchievementCounter.Utilities;

public static class JsonFileUtils {
		private static readonly JsonSerializerSettings _options
			= new() { NullValueHandling = NullValueHandling.Ignore, 
						Formatting = Formatting.Indented };
    
		public static void SimpleWrite(object? obj, string fileName) {
			var jsonString = JsonConvert.SerializeObject(obj, _options);
			File.WriteAllText(fileName, jsonString);
		}

		public static void SimpleRead<T>(string fileName, out T? obj) {
			using var r = new StreamReader(fileName);
			obj = JsonConvert.DeserializeObject<T>(r.ReadToEnd());
		}
}