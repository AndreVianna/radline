namespace RadLine;

public interface ITextEditor {
    ITextCompletion? Completion { get; }
    string ErrorMessage { get; }
    IHighlighter? Highlighter { get; }
    ITextEditorHistory History { get; }
    KeyBindings KeyBindings { get; }
    ILineDecorationRenderer? LineDecorationRenderer { get; }
    uint MaximumNumberOfLines { get; }
    uint PageSize { get; }
    IPromptPrefix PromptPrefix { get; }
    TextEditorState State { get; }
    string Text { get; }

    Func<string, ValidationResult>? Validator { get; }

    Task<string?> ReadText(CancellationToken cancellationToken);
    void ResetState();
}
