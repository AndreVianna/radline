namespace RadLine.Examples;

public sealed class TestCompletion : ITextCompletion {
    public IEnumerable<string> GetCompletions(string context, string word, string suffix)
        => string.IsNullOrWhiteSpace(context)
            ? ["git", "code", "vim"]
            : context.Equals("git ", StringComparison.Ordinal)
                ? ["init", "branch", "push", "commit", "rebase"]
                : null;
}