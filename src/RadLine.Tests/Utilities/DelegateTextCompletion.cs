namespace RadLine.Utilities;

public sealed class DelegateTextCompletion(Func<string, string, string, IEnumerable<string>?> callback) : ITextCompletion
{
    private readonly Func<string, string, string, IEnumerable<string>?> _callback = callback ?? throw new ArgumentNullException(nameof(callback));

    public IEnumerable<string>? GetCompletions(string context, string word, string suffix) => _callback switch
    {
        null => [],
        _ => _callback(context, word, suffix),
    };
}
