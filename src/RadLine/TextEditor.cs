namespace RadLine;

public sealed class TextEditor
    : IHighlighterAccessor,
      ITextEditor {
    private readonly IServiceProvider? _provider;
    private readonly IAnsiConsole _console;
    private readonly TextEditorRenderer _renderer;
    private readonly TextEditorHistory _history;
    private readonly InputBuffer _input;
    private bool IsMultiLine => MaximumNumberOfLines != 1;

    public ITextEditorHistory History => _history;
    public KeyBindings KeyBindings { get; }

    public TextEditorState State { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;

    public uint PageSize { get; }
    public uint MaximumNumberOfLines { get; init; }
    public string Text { get; init; } = string.Empty;
    public IPromptPrefix PromptPrefix { get; init; } = new DefaultPromptPrefix();
    public ITextCompletion? Completion { get; init; }
    public IHighlighter? Highlighter { get; init; }
    public Func<string, ValidationResult>? Validator { get; init; }
    public ILineDecorationRenderer? LineDecorationRenderer { get; init; }

    public TextEditor(IAnsiConsole? terminal = null, IInputSource? source = null, IServiceProvider? provider = null) {
        _console = terminal ?? AnsiConsole.Console;
        _provider = provider;
        PageSize = (uint)_console.Profile.Height;
        _renderer = new(_console, this);
        _history = new();
        _input = new(source ?? new DefaultInputSource(_console));

        KeyBindings = KeyBindings.Default;
    }

    public static bool IsSupported(IAnsiConsole console) => console is null
            ? throw new ArgumentNullException(nameof(console))
            : console.Profile.Out.IsTerminal && console.Profile.Capabilities is { Ansi: true, Interactive: true };

    public async Task<string?> ReadText(CancellationToken cancellationToken) {
        var state = new TextEditorContent(PromptPrefix, Text);
        _history.Reset();
        _input.Initialize(KeyBindings);
        _renderer.Initialize(state);
        var result = await ProcessInput(state, cancellationToken);
        while (result is null)
            result = await ProcessInput(state, cancellationToken);

        return result;
    }

    private async Task<string?> ProcessInput(TextEditorContent content, CancellationToken cancellationToken) {
        var provider = new DefaultServiceProvider(_provider);
        provider.RegisterOptional<ITextCompletion, ITextCompletion>(Completion);
        var context = new LineBufferContext(content.Buffer, provider);
        var submitAction = await ReadInput(context, content, cancellationToken).ConfigureAwait(false);
        switch (submitAction) {
            case SubmitAction.Cancel:
                State = TextEditorState.Cancel;
                return Text;
            case SubmitAction.Submit:
                State = TextEditorState.Ok;
                var validationResult = Validator?.Invoke(content.Text) ?? ValidationResult.Success();
                if (!validationResult.Successful) {
                    ErrorMessage = validationResult.Message ?? string.Empty;
                    State = TextEditorState.Invalid;
                }

                _renderer.RenderLine(content, cursorPosition: 0);
                while (content.MoveDown()) _console.Cursor.MoveDown();
                _console.WriteLine();
                if (!content.IsEmpty) _history.Add(content.GetBuffers());
                return content.Text;
            case SubmitAction.NewLine:
                if (!IsMultiLine || (MaximumNumberOfLines > 1 && content.LineCount >= MaximumNumberOfLines)) break;
                var end = content.Buffer.Content[content.Buffer.Position..];
                content.Buffer.Clear(content.Buffer.Position, content.Buffer.Length);
                if (content.IsLastLine) content.AppendLine(end);
                else content.InsertLine(content.LineIndex + 1, end);
                content.MoveDown();
                content.Buffer.SetPosition(0);
                Refresh(content);
                break;
            case SubmitAction.Delete:
                if (context.Buffer.Position >= content.Buffer.Length) {
                    if (content.IsLastLine) break;
                    content.Buffer.Insert(content.GetBufferAt(content.LineIndex + 1).Content);
                    content.RemoveLine(content.LineIndex + 1);
                    Refresh(content, 1);
                    break;
                }
                context.Buffer.Clear(context.Buffer.Position, 1);
                RenderLine(content);
                break;
            case SubmitAction.Backspace:
                if (context.Buffer.Position <= 0) {
                    if (content.IsFirstLine) break;
                    MoveUp(content);
                    content.Buffer.MoveLineEnd();
                    content.Buffer.Insert(content.GetBufferAt(content.LineIndex + 1).Content);
                    content.RemoveLine(content.LineIndex + 1);
                    Refresh(content, 1);
                    break;
                }
                context.Buffer.Clear(context.Buffer.Position - 1, 1, true);
                RenderLine(content);
                break;
            case SubmitAction.MoveLeft:
                if (context.Buffer.Position <= 0) {
                    if (content.IsFirstLine) break;
                    MoveUp(content);
                    content.Buffer.MoveLineEnd();
                    RenderLine(content, content.Buffer.Position);
                    break;
                }
                context.Buffer.MoveLeft();
                RenderLine(content);
                break;
            case SubmitAction.MoveRight:
                if (context.Buffer.Position >= content.Buffer.Length) {
                    if (content.IsLastLine) break;
                    MoveDown(content);
                    content.Buffer.MoveLineStart();
                    RenderLine(content, 0);
                    break;
                }
                context.Buffer.MoveRight();
                RenderLine(content);
                break;
            case SubmitAction.MoveToPreviousWord:
                if (context.Buffer.Position <= 0 && !content.IsFirstLine) {
                    MoveUp(content);
                    content.Buffer.MoveLineEnd();
                    RenderLine(content, content.Buffer.Position);
                    break;
                }
                context.Buffer.MoveToPreviousWord();
                RenderLine(content);
                break;
            case SubmitAction.MoveToNextWord:
                if (context.Buffer.Position >= content.Buffer.Length && !content.IsLastLine) {
                    MoveDown(content);
                    content.Buffer.MoveLineStart();
                    RenderLine(content, 0);
                    break;
                }
                context.Buffer.MoveToNextWord();
                RenderLine(content);
                break;
            case SubmitAction.MoveTextStart:
                if (IsMultiLine) MoveTop(content);
                context.Buffer.MoveLineStart();
                RenderLine(content);
                break;
            case SubmitAction.MoveTextEnd:
                if (IsMultiLine) MoveBottom(content);
                context.Buffer.MoveLineEnd();
                RenderLine(content);
                break;
            case SubmitAction.MoveLineStart:
                context.Buffer.MoveLineStart();
                RenderLine(content);
                break;
            case SubmitAction.MoveLineEnd:
                context.Buffer.MoveLineEnd();
                RenderLine(content);
                break;
            case SubmitAction.MoveUp:
                if (IsMultiLine) MoveUp(content);
                break;
            case SubmitAction.MoveDown:
                if (IsMultiLine) MoveDown(content);
                break;
            case SubmitAction.MovePageUp:
                if (IsMultiLine) MovePageUp(content);
                break;
            case SubmitAction.MovePageDown:
                if (IsMultiLine) MovePageDown(content);
                break;
            case SubmitAction.MoveTop:
                if (IsMultiLine) MoveTop(content);
                break;
            case SubmitAction.MoveBottom:
                if (IsMultiLine) MoveBottom(content);
                break;
            case SubmitAction.PreviousHistory:
                if (_history.MovePrevious(content)) SetContent(content, [.. _history.Current?.Select(l => l.Content) ?? []]);
                break;
            case SubmitAction.NextHistory:
                if (_history.MoveNext()) SetContent(content, [.. _history.Current?.Select(l => l.Content) ?? []]);
                break;
            default:
                throw new InvalidOperationException("Unknown action.");
        }

        return null;
    }

    public void ResetState() {
        State = TextEditorState.Active;
        ErrorMessage = string.Empty;
    }

    private async Task<SubmitAction> ReadInput(LineBufferContext context, TextEditorContent content, CancellationToken cancellationToken) {
        while (true) {
            if (cancellationToken.IsCancellationRequested) return SubmitAction.Cancel;

            var key = await _input.ReadKey(IsMultiLine, cancellationToken).ConfigureAwait(false);
            var command = key == null
                ? null
                : key.Value.KeyChar != 0 && !char.IsControl(key.Value.KeyChar)
                    ? new InsertCommand(key.Value.KeyChar)
                    : KeyBindings.GetCommand(key.Value.Key, key.Value.Modifiers);

            if (command != null) context.Execute(command);
            if (context.Result != null) return context.Result.Value;
            RenderLine(content);
        }
    }

    private void RenderLine(TextEditorContent content, int? cursorPosition = null) {
        _renderer.RenderLine(content, cursorPosition);
        LineDecorationRenderer?.RenderLineDecoration(content.Buffer);
    }

    private void Refresh(TextEditorContent content, int trailCount = 0) {
        _renderer.Refresh(content, trailCount);
        LineDecorationRenderer?.RenderLineDecoration(content.Buffer);
    }

    private void MoveUp(TextEditorContent content)
        => Move(content, () => {
            if (!content.MoveUp()) return;
            _console.Cursor.MoveUp();
        });

    private void MoveDown(TextEditorContent content)
        => Move(content, () => {
            if (!content.MoveDown()) return;
            _console.Cursor.MoveDown();
        });

    private void MovePageUp(TextEditorContent content)
        => Move(content, () => {
            var lineCount = 0;
            while (PageSize > lineCount++ && content.MoveUp())
                _console.Cursor.MoveUp();
        });

    private void MovePageDown(TextEditorContent content)
        => Move(content, () => {
            var lineCount = 0;
            while (PageSize > lineCount++ && content.MoveDown())
                _console.Cursor.MoveDown();
        });

    private void MoveTop(TextEditorContent content)
        => Move(content, () => {
            while (content.MoveUp())
                _console.Cursor.MoveUp();
        });

    private void MoveBottom(TextEditorContent content)
        => Move(content, () => {
            while (content.MoveDown())
                _console.Cursor.MoveDown();
        });

    private void Move(TextEditorContent content, Action action) {
        using (_console.HideCursor()) {
            var position = content.Buffer.Position;
            if (content.LineCount > _console.Profile.Height) {
                action();
                Refresh(content);
            }
            else {
                _renderer.RenderLine(content, 0);
                action();
            }

            content.Buffer.SetPosition(position);
            _renderer.RenderLine(content);
        }
    }

    private void SetContent(TextEditorContent content, ICollection<string?> lines) {
        var builder = new StringBuilder();
        _renderer.ContentHandler.SetContent(builder, content, lines);
        _console.WriteAnsi(builder.ToString());
    }
}
