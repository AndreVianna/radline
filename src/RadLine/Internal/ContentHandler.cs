namespace RadLine.Internal;

internal sealed class ContentHandler {
    private readonly IAnsiConsole _console;
    private readonly IHighlighterAccessor _accessor;

    public ContentHandler(IAnsiConsole console, IHighlighterAccessor accessor) {
        ArgumentNullException.ThrowIfNull(accessor);

        _console = console ?? throw new ArgumentNullException(nameof(console));
        _accessor = accessor;
    }

    public void RefreshContent(StringBuilder builder, TextEditorContent content, int trailCount = 0) {
        var cursorPosition = content.Buffer.Position + content.PromptPrefix.GetFor(content.LineIndex).Length;
        BuildText(builder, content, content.LineIndex, cursorPosition, trailCount);
    }

    public void SetContent(StringBuilder builder, TextEditorContent content, IEnumerable<string?> lines) {
        var linesBefore = content.LineCount;
        var lineIndex = content.LineIndex;
        var cursorPosition = content.Buffer.Position + content.PromptPrefix.GetFor(lineIndex).Length;

        content.Clear();
        builder.Clear();
        builder.Append("\u001b[?25l");
        var first = true;
        foreach (var line in lines) {
            content.AppendLine(line);
            if (!first) MoveDown(builder, content);
            first = false;
        }

        var numberOfRemovedLines = linesBefore > content.LineCount ? linesBefore - content.LineCount : 0;
        BuildText(builder, content, lineIndex, cursorPosition, numberOfRemovedLines);
        builder.Append("\u001b[?25h");
    }

    public void AddEmptyLine(StringBuilder builder, TextEditorContent content) {
        builder.Append("\u001b[?25l");
        MoveDown(builder, content);
        var cursorPosition = content.Buffer.Position + content.PromptPrefix.GetFor(content.LineIndex).Length;
        BuildText(builder, content, content.LineIndex, cursorPosition, 0);
        builder.Append("\u001b[?25h");
    }

    public void SetLineContent(StringBuilder builder, TextEditorContent content, LineBuffer buffer, int? lineIndex = null, int? cursorPosition = null) {
        builder.Append("\u001b[?25l");
        builder.Append("\u001b[2K"); // Clear the current line
        builder.Append("\u001b[1G"); // Set cursor to beginning of line

        var prompt = content.PromptPrefix.GetFor(lineIndex ?? content.LineIndex);
        cursorPosition ??= content.Buffer.Position;
        builder.Append(_console.ToAnsi(prompt));

        var width = _console.Profile.Width - prompt.Length - 1;
        var (line, position) = BuildLine(buffer, width, cursorPosition.Value);
        var text = _console.ToAnsi(BuildHighlightedText(line));
        if (line.Length < width) {
            text = text.PadRight(width);
        }
        builder.Append(text);

        // Move the cursor to the right position
        var cursorPos = position + prompt.Length + 1;
        builder.Append("\u001b[").Append(cursorPos).Append('G');
        builder.Append("\u001b[?25h");
    }

    private void MoveDown(StringBuilder builder, TextEditorContent content) {
        if (content.LineCount > _console.Profile.Height) {
            builder.Append("\u001b[1B");
        }
        else {
            builder.AppendLine();
        }
    }

    private void BuildText(StringBuilder builder, TextEditorContent content, int cursorLine, int cursorPosition, int trailCount) {
        for (var lineUp = 0; lineUp < content.LineIndex; lineUp++)
            builder.Append("\u001bM"); // moves cursor one line up, scrolling if needed

        for (var line = 0; line < content.LineCount + trailCount; line++) {
            builder.Append("\u001b[1G"); // Move cursor to beginning of line
            builder.Append("\u001b[2K"); // erase the entire line
            if (line >= content.LineCount) continue;
            SetLineContent(builder, content, content.GetBufferAt(line), line, 0);
            builder.Append("\u001b[1E");
        }

        // Restore cursor position
        var column = cursorPosition + 1;
        builder.Append($"\u001b[{cursorLine + 1};{column}H");
    }

    private static (string Content, int? CursorPosition) BuildLine(LineBuffer buffer, int width, int position) {
        var middleOfList = width / 2;

        var skip = 0;
        var take = buffer.Content.Length;
        var cursorPosition = position;

        var scrollable = buffer.Content.Length > width;
        if (!scrollable) return (string.Concat(buffer.Content.Skip(skip).Take(take)), cursorPosition);

        skip = Math.Max(0, position - middleOfList);
        take = Math.Min(width, buffer.Content.Length - skip);

        if (buffer.Content.Length - position < middleOfList) {
            // Pointer should be below the end of the list
            var diff = middleOfList - (buffer.Content.Length - position);
            skip -= diff;
            take += diff;
            cursorPosition = middleOfList + diff;
        }
        else {
            cursorPosition -= skip;
        }

        return (string.Concat(buffer.Content.Skip(skip).Take(take)), cursorPosition);
    }

    private IRenderable BuildHighlightedText(string text) {
        var highlighter = _accessor.Highlighter;
        return highlighter switch {
            null => new Text(text),
            _ => highlighter.BuildHighlightedText(text),
        };
    }
}
