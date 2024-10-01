namespace RadLine;

public static class ITextCompletionExtensions {
    public static bool TryGetCompletions(
        this ITextCompletion completion,
        string prefix, string word, string suffix,
        [NotNullWhen(true)] out string[]? result) {
        result = completion.GetCompletions(prefix, word, suffix)?.ToArray() ?? [];
        if (result.Length == 0) {
            result = null;
        }

        return result is not null;
    }
}
