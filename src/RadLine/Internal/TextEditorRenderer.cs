namespace RadLine.Internal;

internal sealed class TextEditorRenderer {
    private readonly IAnsiConsole _console;

    internal LineBuilder LineBuilder { get; }

    public TextEditorRenderer(IAnsiConsole console, IHighlighterAccessor accessor) {
        ArgumentNullException.ThrowIfNull(accessor);

        _console = console ?? throw new ArgumentNullException(nameof(console));

        LineBuilder = new(_console, accessor);

        if (!_console.Profile.Capabilities.Ansi) {
            throw new NotSupportedException("Terminal does not support ANSI");
        }
    }

    public void Initialize(TextEditorContent content) {
        // Everything fit inside the terminal?
        if (content.LineCount < _console.Profile.Height) {
            _console.Cursor.Hide();

            var builder = new StringBuilder();
            for (var lineIndex = 0; lineIndex < content.LineCount; lineIndex++) {
                LineBuilder.BuildLine(builder, content, content.GetBufferAt(lineIndex), lineIndex, 0);

                if (lineIndex != content.LineCount - 1) {
                    builder.Append(Environment.NewLine);
                }
            }

            _console.WriteAnsi(builder.ToString());
            _console.Cursor.Show();

            // Move to the last line and refresh it
            content.Move(content.LineCount);
            RenderLine(content);
        }
        else {
            Refresh(content);
        }
    }

    public void Refresh(TextEditorContent content) {
        var builder = new StringBuilder();
        LineBuilder.BuildRefresh(builder, content);
        _console.WriteAnsi(builder.ToString());
    }

    public void RenderLine(TextEditorContent content, int? cursorPosition = null) {
        var builder = new StringBuilder();
        LineBuilder.BuildLine(builder, content, content.Buffer, null, cursorPosition);
        _console.WriteAnsi(builder.ToString());
    }
}
