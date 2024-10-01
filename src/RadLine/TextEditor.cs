namespace RadLine;

public sealed class TextEditor : IHighlighterAccessor {
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

    public uint PageSize { get; init; } = 10;
    public uint MaximumNumberOfLines { get; init; }
    public string Text { get; init; } = string.Empty;
    public IPromptPrefix PromptPrefix { get; init; } = new PromptPrefix("[yellow]>[/]");
    public ITextCompletion? Completion { get; init; }
    public IHighlighter? Highlighter { get; init; }
    public Func<string, ValidationResult>? Validator { get; init; }
    public ILineDecorationRenderer? LineDecorationRenderer { get; init; }

    public TextEditor(IAnsiConsole? terminal = null, IInputSource? source = null, IServiceProvider? provider = null) {
        _console = terminal ?? AnsiConsole.Console;
        _provider = provider;
        _renderer = new(_console, this);
        _history = new();
        _input = new(source ?? new DefaultInputSource(_console));

        KeyBindings = new();
        KeyBindings.AddDefault();
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
                while (content.MoveDown()) {
                    _console.Cursor.MoveDown();
                }
                _console.WriteLine();
                if (!content.IsEmpty) {
                    _history.Add(content.GetBuffers());
                }
                return content.Text;
            case SubmitAction.PreviousHistory:
                if (_history.MovePrevious(content)) {
                    SetContent(content, _history.Current);
                }

                break;
            case SubmitAction.NextHistory:
                if (_history.MoveNext()) {
                    SetContent(content, _history.Current);
                }

                break;
            case SubmitAction.NewLine:
                if (!IsMultiLine || (MaximumNumberOfLines > 1 && content.LineCount >= MaximumNumberOfLines)) {
                    break;
                }

                if (content.IsLastLine) {
                    content.AddLine();
                }

                var builder = new StringBuilder();
                builder.Append("\u001b[?25l");
                _renderer.LineBuilder.MoveDown(builder, content);
                _renderer.LineBuilder.BuildRefresh(builder, content);
                builder.Append("\u001b[?25h");
                _console.WriteAnsi(builder.ToString());
                break;
            case SubmitAction.MoveLeft:
                if (context.Buffer.Position <= 0 && !content.IsFirstLine) {
                    MoveUp(content);
                    content.Buffer.MoveLineEnd();
                    RenderLine(content, content.Buffer.Position);
                    break;
                }
                context.Buffer.MoveLeft();
                RenderLine(content);
                break;
            case SubmitAction.MoveRight:
                if (context.Buffer.Position >= content.Buffer.Length && !content.IsFirstLine) {
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
            if (cancellationToken.IsCancellationRequested) {
                return SubmitAction.Cancel;
            }

            var command = default(TextEditorCommand);
            var key = await _input.ReadKey(IsMultiLine, cancellationToken).ConfigureAwait(false);
            if (key != null) {
                command = key.Value.KeyChar != 0 && !char.IsControl(key.Value.KeyChar)
                    ? new InsertCommand(key.Value.KeyChar)
                    : KeyBindings.GetCommand(key.Value.Key, key.Value.Modifiers);
            }

            if (command != null) {
                context.Execute(command);
            }

            if (context.Result != null) {
                return context.Result.Value;
            }

            RenderLine(content);
        }
    }

    private void RenderLine(TextEditorContent content, int? cursorPosition = null) {
        _renderer.RenderLine(content, cursorPosition);
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
                _renderer.Refresh(content);
            }
            else {
                _renderer.RenderLine(content, 0);
                action();
            }

            content.Buffer.SetPosition(position);
            _renderer.RenderLine(content);
        }
    }

    private void SetContent(TextEditorContent content, IList<LineBuffer>? lines) {
        if (lines == null || lines.Count == 0) {
            return;
        }

        var builder = new StringBuilder();
        LineBuilder.BuildClear(builder, content);
        content.RemoveAllLines();
        builder.Append("\u001b[?25l");

        var first = true;
        foreach (var line in lines) {
            content.AddLine(line.Content);
            if (!first) {
                _renderer.LineBuilder.MoveDown(builder, content);
            }

            first = false;
        }

        _renderer.LineBuilder.BuildRefresh(builder, content);
        builder.Append("\u001b[?25h");
        _console.WriteAnsi(builder.ToString());
    }
}
