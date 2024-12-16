using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace AchievementCounter;

public class Data {
	public string? ApiKey { get; set; }
	public string? SteamId { get; set; }
	public string? AppId { get; set; }
}

internal static class Program
{
	private static Data? _data = new();

	private const string DATA_FILE_NAME = "preferences.json";

	private static readonly string _path =
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\AchievementCounter\";
	private static readonly string _dataPath =
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\AchievementCounter\data\";

	private static readonly HttpClient _client = new();

	private static Task Main() {
		_ = Initialize();
		
		Console.Clear();
		
		while (true) {
			Update();
			Thread.Sleep(1000);
		}
	}

	private static Task Initialize() {
		var firstExecution = !Directory.Exists(_path);
		Console.Title = "Achievement Counter";

		switch (firstExecution) {
			case true:
				Directory.CreateDirectory(_path);
				Directory.CreateDirectory(_dataPath);

				File.Create(_dataPath + DATA_FILE_NAME).Close();
				File.Create(_path+"achievementsCount.txt").Close();
				break;
			case false:
				Console.WriteLine("Do you want to change anything? [Y][N]");
				break;
		}

		if (!firstExecution && Console.Read() != 'Y') return Task.CompletedTask;

		do {
			Console.WriteLine("You can get your API key here: https://steamcommunity.com/dev/apikey");
			Console.WriteLine("Just put 127.0.0.1 into domain field");
			Console.WriteLine("Enter API Key:");

			if (_data != null) _data.ApiKey = Console.ReadLine();

			Console.Clear();
		} while (string.IsNullOrEmpty(_data?.ApiKey));
		
		do {
			Console.WriteLine("How to get steam ID: https://help.bethesda.net/#en/answer/49679");
			Console.WriteLine("Enter Steam ID:");
		
			_data.SteamId = Console.ReadLine();
		
			Console.Clear();
		} while (string.IsNullOrEmpty(_data.SteamId));

		do {
			Console.WriteLine("Enter Y for Stalcraft");
			Console.WriteLine("Enter App ID:");
			
			var input = Console.ReadLine();
			
			_data.AppId = input == "Y" ? "1818450" : input;
			
			Console.Clear();
		} while (string.IsNullOrEmpty(_data.AppId));
		
		JsonFileUtils.SimpleWrite(_data, _dataPath + DATA_FILE_NAME);
		
		Console.WriteLine("Now go to your OBS and add new Text (GDI+) source");
		Console.WriteLine("Then, in the properties window look for \"Read from file\" option and toggle it on");
		Console.WriteLine("Required file's path is <appdata/Roaming/Achievement Counter/achievementsCount.txt>");
		
		Console.WriteLine("\nPress any key to continue");
		
		Console.ReadKey();
		
		return Task.CompletedTask;
	}
	
	private static async void Update() {
		try
		{
			var request =
				$"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?key={_data?.ApiKey}&steamid={_data?.SteamId}&appid={_data?.AppId}";
			
			using var response = await _client.GetAsync(request);
			response.EnsureSuccessStatusCode();
			var responseBody = await response.Content.ReadAsStringAsync();
			
			var obj = JObject.Parse(responseBody);

			var count = 0;
			var maxCount = 0;
			
			foreach (var achievements in obj) {
				foreach (var achievement in achievements.Value!["achievements"]!) {
					maxCount++;
					count += achievement["achieved"]!.ToString() == "1"? 1 : 0;
				}
			}
			
			WriteCount($"{count}/{maxCount}");
			
			Console.WriteLine($"{count}/{maxCount}");
		}
		catch (HttpRequestException e)
		{
			Console.WriteLine("\nException Caught!");
			Console.WriteLine("Message :{0} ", e.Message);
		}
	}
	
	private static async void WriteCount(string count) {
		await using var outputFile = new StreamWriter(Path.Combine(_path, "achievementsCount.txt"));
		await outputFile.WriteLineAsync(count);
	}
}