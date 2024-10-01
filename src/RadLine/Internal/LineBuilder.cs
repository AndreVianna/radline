namespace RadLine.Internal;

internal sealed class LineBuilder {
    private readonly IAnsiConsole _console;
    private readonly IHighlighterAccessor _accessor;

    public LineBuilder(IAnsiConsole console, IHighlighterAccessor accessor) {
        ArgumentNullException.ThrowIfNull(accessor);

        _console = console ?? throw new ArgumentNullException(nameof(console));
        _accessor = accessor;
    }

    public static void BuildClear(StringBuilder builder, TextEditorContent content) {
        if (content.LineIndex != 0) {
            // Move the cursor to the top
            builder.Append("\u001b[").Append(content.LineIndex).Append('A');
        }

        // Clear lines
        for (var i = 0; i < content.LineCount; i++) {
            builder.Append("\u001b[2K");

            if (i != content.LineCount - 1) {
                // Move down
                builder.Append("\u001b[1B");
            }
        }

        // Move back to the top
        var position = content.LineCount - 1;
        if (position > 0) {
            builder.Append("\u001b[").Append(position).Append('A');
        }

        // Set cursor to beginning of line
        builder.Append("\u001b[1G");
    }

    public void BuildRefresh(StringBuilder builder, TextEditorContent content) {
        if (content.LineCount > _console.Profile.Height) {
            BuildFullDisplayRefresh(builder, content);
        }
        else {
            BuildPartialDisplayRefresh(builder, content);
        }
    }

    public void BuildLine(StringBuilder builder, TextEditorContent content, LineBuffer buffer, int? lineIndex = null, int? cursorPosition = null) {
        builder.Append("\u001b[2K"); // Clear the current line
        builder.Append("\u001b[1G"); // Set cursor to beginning of line

        var (prompt, margin) = content.PromptPrefix.GetPrompt(content, lineIndex ?? content.LineIndex);

        cursorPosition ??= content.Buffer.Position;

        // Render the prompt
        builder.Append(_console.ToAnsi(prompt));
        builder.Append(new string(' ', margin));

        // Build the buffer
        var width = _console.Profile.Width - prompt.Length - margin - 1;
        var (line, position) = BuildLineContent(buffer, width, cursorPosition.Value);

        // Build the resulting ANSI
        var text = _console.ToAnsi(BuildHighlightedText(line));

        // Need to pad the content?
        if (line.Length < width) {
            text = text.PadRight(width);
        }

        // Output the buffer
        builder.Append(text);

        // Move the cursor to the right position
        var cursorPos = position + prompt.Length + margin + 1;
        builder.Append("\u001b[").Append(cursorPos).Append('G');
    }

    public void MoveDown(StringBuilder builder, TextEditorContent content) {
        if (content.LineCount > _console.Profile.Height) {
            builder.Append("\u001b[1B");
        }
        else {
            builder.AppendLine();
        }
    }

    private void BuildFullDisplayRefresh(StringBuilder builder, TextEditorContent content) {
        builder.Append("\u001b[1;1H"); // Set cursor to home position

        var lineIndex = 0;
        var height = _console.Profile.Height;
        var lineCount = content.LineCount - 1;
        var middleOfList = height / 2;
        var offset = height % 2 == 0 ? 1 : 0;
        var pointer = content.LineIndex;

        // Calculate the visible part
        var scrollable = lineCount >= height;
        if (scrollable) {
            var skip = Math.Max(0, content.LineIndex - middleOfList);

            if (lineCount - content.LineIndex < middleOfList) {
                // Pointer should be below the end of the list
                var diff = middleOfList - (lineCount - content.LineIndex);
                skip -= diff - offset;
                pointer = middleOfList + diff - offset;
            }
            else {
                // Take skip into account
                pointer -= skip;
            }

            lineIndex = skip;
        }

        // Render all lines
        for (var i = 0; i < Math.Min(content.LineCount, height); i++) {
            // Set cursor to beginning of line
            builder.Append("\u001b[1G");

            // Render the line
            BuildLine(builder, content, content.GetBufferAt(lineIndex), lineIndex, 0);

            // Move cursor down
            builder.Append("\u001b[1E");
            lineIndex++;
        }

        // Position the cursor at the right line
        builder.Append("\u001b[").Append(pointer + 1).Append(";1H");

        // Refresh the current line
        BuildLine(builder, content, content.Buffer);
    }

    private void BuildPartialDisplayRefresh(StringBuilder builder, TextEditorContent content) {
        if (content.LineIndex > 0) {
            // Move the cursor up
            builder.Append("\u001b[").Append(content.LineIndex).Append('A');
        }

        // Render all lines
        for (var lineIndex = 0; lineIndex < content.LineCount; lineIndex++) {
            // Set cursor to beginning of line
            builder.Append("\u001b[1G");

            // Render the line
            BuildLine(builder, content, content.GetBufferAt(lineIndex), lineIndex, 0);
            builder.Append('\n');
        }

        // Position the cursor at the right line
        var moveUp = content.LineCount - content.LineIndex;
        builder.Append("\u001b[").Append(moveUp).Append('A');

        // Refresh the current line
        BuildLine(builder, content, content.Buffer);
    }

    private static (string Content, int? Cursor) BuildLineContent(LineBuffer buffer, int width, int position) {
        var middleOfList = width / 2;

        var skip = 0;
        var take = buffer.Content.Length;
        var pointer = position;

        var scrollable = buffer.Content.Length > width;
        if (!scrollable) return (string.Concat(buffer.Content.Skip(skip).Take(take)), pointer);

        skip = Math.Max(0, position - middleOfList);
        take = Math.Min(width, buffer.Content.Length - skip);

        if (buffer.Content.Length - position < middleOfList) {
            // Pointer should be below the end of the list
            var diff = middleOfList - (buffer.Content.Length - position);
            skip -= diff;
            take += diff;
            pointer = middleOfList + diff;
        }
        else {
            pointer -= skip;
        }

        return (string.Concat(buffer.Content.Skip(skip).Take(take)), pointer);
    }

    private IRenderable BuildHighlightedText(string text) {
        var highlighter = _accessor.Highlighter;
        return highlighter switch {
            null => new Text(text),
            _ => highlighter.BuildHighlightedText(text),
        };
    }
}
