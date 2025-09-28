namespace AchievementCounter.Utilities;

public enum AnswerKind {
    Skip,
    No,
    Yes,
    Complicated,
}

public struct Answer {
    public readonly AnswerKind Kind;
    public readonly string Ans;
    
    
    public Answer(string ans, AnswerKind kind) {
        Ans = ans;
        Kind = kind;
    }
} 

public static class SurveyUtility { 
    public static Answer Ask(string question) {
        var kind = AnswerKind.Skip;
        string? ans;

        do {
            Console.Clear();
            Console.Write(question);
            ans = Console.ReadLine();
            if (string.IsNullOrEmpty(ans)) {
                ans = string.Empty;
                break;
            }

            if (ans.Equals("y", StringComparison.InvariantCultureIgnoreCase)) {
                kind = AnswerKind.Yes;
                break;
            }

            if (ans.Equals("n", StringComparison.InvariantCultureIgnoreCase)) {
                kind = AnswerKind.No;
                break;
            }

            kind = AnswerKind.Complicated;
            break;
        } while (false);

        Console.Clear();
        return new Answer(ans ?? string.Empty, kind);
    }
}