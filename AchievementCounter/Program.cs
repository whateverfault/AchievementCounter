using AchievementCounter.Utilities;
using Newtonsoft.Json.Linq;

namespace AchievementCounter;

public class Data {
	public string? ApiKey { get; set; }
	public string? SteamId { get; set; }
	public string? AppId { get; set; }
	public string? Message { get; set; }
	public string? Layout { get; set; }
}

internal static class Program
{
	private static Data? _data = new();
	
	private const string DATA_FILE_NAME = "preferences.json";
	private const string FILE_NAME = "achievementsCount.txt";
	
	private const char SKIP_CHAR = '~';
	
	private static readonly string _path =
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\AchievementCounter\";
	private static readonly string _dataPath =
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\AchievementCounter\data\";

	private static Task Main() {
		Console.Clear();
		
		Initialize();
		
		Console.Clear();
		
		while (true) {
			Update();
			Thread.Sleep(10000);
		}
	}

	private static void Initialize() {
		Console.Title = "Achievement Counter";
		
		ConsoleUtility.Hide();
		
		var firstExecution = !Directory.Exists(_path);

		var ans = string.Empty;
		
		switch (firstExecution) {
			case true:
				Directory.CreateDirectory(_path);
				Directory.CreateDirectory(_dataPath);

				File.Create(_dataPath + DATA_FILE_NAME).Close();
				File.Create(_path+FILE_NAME).Close();
				break;
			case false:
				do {
					ans = SurveyUtility.Ask($"Do you want to change something? [Y][N][{SKIP_CHAR} - skip]");
				} while (string.IsNullOrEmpty(ans));
				
				break;
		}

		if (!firstExecution && !ans.Equals("Y", StringComparison.CurrentCultureIgnoreCase)) {
			JsonFileUtils.SimpleRead(_dataPath+DATA_FILE_NAME, out _data);
			return;
		}

		JsonFileUtils.SimpleRead<Data>(_dataPath + DATA_FILE_NAME, out var jsonData);
		
		string? input;
		do {
			_data!.ApiKey = SurveyUtility.Ask(
				"You can get your API key here: https://steamcommunity.com/dev/apikey" +
				"\nJust put 127.0.0.1 into domain field" +
				"\nEnter App ID:",
				jsonData?.ApiKey)?.Replace(" ", "");
			
		} while (string.IsNullOrEmpty(_data.ApiKey));
		Console.Clear();
		
		do {
			_data.SteamId = SurveyUtility.Ask(
				"How to get steam ID: https://help.bethesda.net/#en/answer/49679" +
				"\nEnter Steam ID:",
				jsonData?.SteamId)?.Replace(" ", "");
		} while (string.IsNullOrEmpty(_data.SteamId));
		
		do {
			input = SurveyUtility.Ask(
				"Enter Y for Stalcraft" +
				"\nEnter App ID:",
				jsonData?.AppId)?.Replace(" ", "");
			
			_data.AppId = input == "Y" ? "1818450" : input;
		} while (string.IsNullOrEmpty(_data.AppId));

		var messageInput = SurveyUtility.Ask("Enter Message:"
			,jsonData?.Message)?.Replace(" ", "");
		_data.Message = messageInput;
		
		int layout;
		do {
			input = SurveyUtility.Ask(
				"1 - default layout" +
						"\n2 - free layout" +
						"\n3 - foxtailo's layout" +
						"\nChoose Option:",
						jsonData?.Layout)?.Replace(" ", "");
		} while (!int.TryParse(input, out layout));
		_data.Layout = layout.ToString();
		
		Console.Clear();
		
		JsonFileUtils.SimpleWrite(_data, _dataPath + DATA_FILE_NAME);
		
		Console.WriteLine("Now go to your OBS and add new Text (GDI+) source");
		Console.WriteLine("Then, in the properties window look for \"Read from file\" option and toggle it on");
		Console.WriteLine("Required file's path is <appdata/Roaming/Achievement Counter/achievementsCount.txt>");
		
		Console.WriteLine("\nPress any key to continue");
		
		Console.ReadKey();
	}
	
	private static async void Update() {
		try
		{
			var request =
				$"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?key={_data?.ApiKey}&steamid={_data?.SteamId}&appid={_data?.AppId}";

			var client = new HttpClient();
			
			using var response = await client.GetAsync(request);
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

			var offset = " ";
			var offsetCount = (_data?.Message?.Length)/2-(count!.ToString().Length+maxCount.ToString().Length)/2 ?? 0;
			for (var i = 0; i < MathF.Abs(offsetCount); i++) {
				offset += " ";
			}
			
			switch (_data?.Layout) {
				case "1":
					WriteCount($"{count}/{maxCount}{_data.Message}");
					break;
				case "2":
					WriteCount($"{count}/{maxCount}\n{_data.Message}");
					break;
				case "3":
					WriteCount($"{(offsetCount > 0? offset : "")}{count}/{maxCount}\n{(offsetCount < 0? offset : "")}{_data.Message}");
					break;
			}
			
		}
		catch (HttpRequestException e)
		{
			Console.WriteLine("Exception Caught!");
			Console.WriteLine("Message: {0} ", e.Message);
			Console.ReadKey();
			Environment.Exit((int)e.StatusCode!);
		}
	}
	
	private static async void WriteCount(string count) {
		await using var outputFile = new StreamWriter(_path+FILE_NAME);
		await outputFile.WriteLineAsync(count);
		outputFile.Close();
	}
}