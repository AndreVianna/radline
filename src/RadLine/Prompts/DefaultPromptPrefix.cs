namespace RadLine.Prompts;

public sealed class DefaultPromptPrefix : IPromptPrefix {
    private readonly Markup _firstLine;
    private readonly Markup _otherLines;

    public DefaultPromptPrefix()
        : this(">") {
    }

    public DefaultPromptPrefix(string prompt)
        : this(prompt ?? throw new ArgumentNullException(nameof(prompt)), prompt) {
    }

    public DefaultPromptPrefix(string firstLine, string otherLines) {
        ArgumentNullException.ThrowIfNull(firstLine, nameof(firstLine));
        _firstLine = new(firstLine + ' ');
        if (_firstLine.Lines > 1) throw new ArgumentException("Prompt cannot contain line breaks", nameof(firstLine));
        ArgumentNullException.ThrowIfNull(otherLines, nameof(otherLines));
        _otherLines = new(otherLines + ' ');
        if (_otherLines.Lines > 1) throw new ArgumentException("Prompt cannot contain line breaks", nameof(otherLines));
    }

    public Markup GetFor(int line) => line switch {
        0 => _firstLine,
        _ => _otherLines,
    };
}
