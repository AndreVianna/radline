namespace RadLine.Prompts;

public sealed class PromptPrefix : IPromptPrefix {
    private readonly Markup _prompt;
    private readonly Markup? _more;

    public PromptPrefix(string prompt, string? more = null) {
        ArgumentNullException.ThrowIfNull(prompt);

        _prompt = new(prompt);
        _more = more != null ? new Markup(more) : null;

        if (_prompt.Lines > 1) {
            throw new ArgumentException("Prompt cannot contain line breaks", nameof(prompt));
        }

        if (_more?.Lines > 1) {
            throw new ArgumentException("Prompt cannot contain line breaks", nameof(more));
        }
    }

    public (Markup Markup, int Margin) GetPrompt(ITextEditorContent content, int line) => line switch {
        0 => (_prompt, 1),
        _ => (_more ?? _prompt, 1),
    };
}
