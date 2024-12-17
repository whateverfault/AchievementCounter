namespace AchievementCounter.Utilities;

public static class SurveyUtility {
	public static string? Ask(string question, string? @default = null, char skipChar = '~') {
		Console.Clear();
		Console.WriteLine(question);
		var input = Console.ReadLine();
		if (input == skipChar.ToString()) input = @default;
		return input;
	}
}