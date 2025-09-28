using System.Text;
using AchievementCounter.Response;
using AchievementCounter.Utilities;
using Newtonsoft.Json;

namespace AchievementCounter;

public enum LogLevel {
    Info,
    Warning,
    Error,
}

internal static class Program {
    private static readonly HttpClient _client = new HttpClient();
    private static SaveData? _data;

    private const string DATA_FILE_NAME = "preferences.json";
    private const string FILE_NAME = "achievementsCount.txt";
    
    private static readonly string _dataFolder = Path.Combine(Environment.CurrentDirectory, "data");

    private static async Task Main() {
        Console.Clear();
        if (!Initialize()) {
            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
            return;
        }
        Console.Clear();

        if (_data == null) {
            Console.WriteLine("Failed to load saved data.\n \nPress any key to exit.");
            Console.ReadKey();
            return;
        }
        
        while (true) {
            await Update();
            Thread.Sleep(_data.Cooldown);
        }
    }

    private static bool Initialize() {
        Console.Title = "Achievement Counter";
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        
        var firstExecution = !Directory.Exists(_dataFolder);

        Answer ans;
        switch (firstExecution) {
            case true:
                Directory.CreateDirectory(_dataFolder);

                File.Create(Path.Combine(_dataFolder, DATA_FILE_NAME)).Close();
                File.Create(Path.Combine(_dataFolder, FILE_NAME)).Close();
                ans = new Answer(string.Empty, AnswerKind.Yes);
                break;
            case false:
                ans = SurveyUtility.Ask($"Do you want to change something? [y/N]: ");
                break;
        }
        
        if (!firstExecution && ans.Kind is AnswerKind.No or AnswerKind.Skip) {
            Json.Read(Path.Combine(_dataFolder, DATA_FILE_NAME), out _data);
            if (_data == null) {
                Console.WriteLine("Failed to read save files.");
                return false;
            }
            
            WinApi.HideWindow(WinApi.GetConsoleWindow());
            return true;
        }

        Json.Read(Path.Combine(_dataFolder, DATA_FILE_NAME), out SaveData? jsonData);

        jsonData ??= new SaveData();
        _data ??= new SaveData();
        _data.Debug = jsonData.Debug;
        
        ans = SurveyUtility.Ask(
                                "You can get your API key here: https://steamcommunity.com/dev/apikey\n" +
                                "Enter API key: "
                                );
        _data.ApiKey = ans.Kind == AnswerKind.Skip? 
                           jsonData.ApiKey :
                           ans.Ans.Replace(" ", "");

        ans = SurveyUtility.Ask(
                                "How to get steam ID: https://help.bethesda.net/#en/answer/49679\n" + 
                                "Enter Steam ID: "
                                );
        _data.SteamId = ans.Kind == AnswerKind.Skip? 
                            jsonData.SteamId : 
                            ans.Ans.Replace(" ", "");

        ans = SurveyUtility.Ask("Enter App ID: ");
        _data.AppId = ans.Kind == AnswerKind.Skip? 
                          jsonData.AppId : 
                          ans.Ans.Replace(" ", "");

        ans = SurveyUtility.Ask("Enter Message: ");
        _data.Message = ans.Ans;

        ans = SurveyUtility.Ask(
                                "1 - default layout\n" +
                                "2 - free layout\n" + 
                                "3 - foxtailo's layout\n" + 
                                "Choose Option: "
                               );
        _data.Layout = int.TryParse(ans.Ans, out var layout)?
                           layout :
                           1;
        
        Console.Clear();
        Json.Write(_data, Path.Combine(_dataFolder, DATA_FILE_NAME));
        
        Console.WriteLine("Now go to your OBS and add new Text (GDI+) source");
        Console.WriteLine("Then, in the properties window look for \"Read from file\" option and toggle it on");
        Console.WriteLine("Required file's path is <appdata/Roaming/Achievement Counter/achievementsCount.txt>");
        
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
        WinApi.HideWindow(WinApi.GetConsoleWindow());
        return true;
    }

    private static async Task Update() {
        try {
            if (_data == null
             || string.IsNullOrEmpty(_data.ApiKey)
             || string.IsNullOrEmpty(_data.SteamId)
             || string.IsNullOrEmpty(_data.AppId)) {
                Log(LogLevel.Error, "Failed to fetch player achievements. Not Initialized.");
                return;
            }
            
            var request =
                $"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?key={_data.ApiKey}&steamid={_data.SteamId}&appid={_data.AppId}";
            
            using var response = await _client.GetAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode) {
                Log(LogLevel.Error, $"Failed to fetch player achievements. Status: {response.StatusCode}\n Content: {content}");
                return;
            }
            
            var deserialized = JsonConvert.DeserializeObject<GetPlayerAchievementsResponse>(content);
            if (deserialized == null) {
                Log(LogLevel.Error, "Failed to parse the response.");
                return;
            }
            
            var count = 0;
            var maxCount = 0;
            foreach (var achievement in deserialized.PlayerStats.Achievements) {
                ++maxCount;
                count += achievement.Achieved? 1 : 0;
            }

            var offset = "";
            var offsetCount = _data.Message?.Length-2-(count.ToString().Length+maxCount.ToString().Length+1)/2 ?? 0;
            for (var i = 0; i < MathF.Abs(offsetCount); i++) {
                offset += " ";
            }

            var message = string.IsNullOrEmpty(_data.Message)?
                              string.Empty :
                              _data.Message; 
            switch (_data.Layout) {
                case 1:
                    WriteCount($"{count}/{maxCount} - {message}");
                    break;
                case 2:
                    WriteCount($"{count}/{maxCount}\n{message}");
                    break;
                case 3: 
                    WriteCount($"{(offsetCount > 0? offset : "")}{count}/{maxCount}\n{(offsetCount < 0? offset : "")}{message}"); 
                    break; 
            }

            if (!_data.Debug) return;
            Log(LogLevel.Info, $"Cooldown: {_data.Cooldown}");
            Log(LogLevel.Info, $"{count}/{maxCount} | {_data.AppId} | {_data.SteamId} | {_data.ApiKey} | {_data.Message}");
        } catch (Exception e) {
            Log(LogLevel.Error, e.Message);
        }
    }

    private static async void WriteCount(string count) {
        try {
            await using var outputFile = new StreamWriter(Path.Combine(_dataFolder, FILE_NAME)); 
            await outputFile.WriteLineAsync(count); 
            outputFile.Close();
        }catch (Exception e) {
            Log(LogLevel.Error, e.Message);
        }
    }

    private static void Log(LogLevel logLevel, string message) {
        var now = DateTimeOffset.Now;
        
        Console.Write($"[{now}] ");
        switch (logLevel) {
            case LogLevel.Info: {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("INFO: ");
                break;
            }
            case LogLevel.Warning: {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("WARNING: ");
                break;
            }
            case LogLevel.Error: {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("ERROR: ");
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
        
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;
    }
}