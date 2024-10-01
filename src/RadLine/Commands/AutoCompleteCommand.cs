namespace RadLine.Commands;

public sealed class AutoCompleteCommand(AutoComplete kind) : TextEditorCommand {
    private const string Position = nameof(Position);
    private const string Index = nameof(Index);

    private readonly AutoComplete _kind = kind;

    public override void Execute(LineBufferContext lineBufferContext) {
        var completion = lineBufferContext.GetService<ITextCompletion>();
        if (completion == null) {
            return;
        }

        var originalPosition = lineBufferContext.Buffer.Position;

        // Get the start position of the word
        var start = lineBufferContext.Buffer.Position;
        if (lineBufferContext.Buffer.IsAtCharacter) {
            lineBufferContext.Buffer.MoveToBeginningOfWord();
            start = lineBufferContext.Buffer.Position;
        }
        else if (lineBufferContext.Buffer.IsAtEndOfWord) {
            lineBufferContext.Buffer.MoveToPreviousWord();
            start = lineBufferContext.Buffer.Position;
        }

        // Get the end position of the word.
        var end = lineBufferContext.Buffer.Position;
        if (lineBufferContext.Buffer.IsAtCharacter) {
            lineBufferContext.Buffer.MoveToEndOfWord();
            end = lineBufferContext.Buffer.Position;
        }

        // Not the same start position as last time?
        if (lineBufferContext.GetState(Position, () => 0) != start) {
            // Reset
            var startIndex = _kind == AutoComplete.Next ? 0 : -1;
            lineBufferContext.SetState(Index, startIndex);
        }

        // Get the prefix and word
        var prefix = lineBufferContext.Buffer.Content[..start];
        var word = lineBufferContext.Buffer.Content[start..end];
        var suffix = lineBufferContext.Buffer.Content[end..];

        // Get the completions
        if (!completion.TryGetCompletions(prefix, word, suffix, out var completions)) {
            lineBufferContext.Buffer.SetPosition(originalPosition);
            return;
        }

        // Get the index to insert
        var index = GetSuggestionIndex(lineBufferContext, word, completions);
        if (index == -1) {
            lineBufferContext.Buffer.SetPosition(originalPosition);
            return;
        }

        // Remove the current word
        if (start != end) {
            lineBufferContext.Buffer.Clear(start, end - start);
            lineBufferContext.Buffer.SetPosition(start);
        }

        // Insert the completion
        lineBufferContext.Buffer.Insert(completions[index]);

        // Move to the end of the word
        lineBufferContext.Buffer.MoveToEndOfWord();

        // Increase the completion index
        lineBufferContext.SetState(Position, start);
        lineBufferContext.SetState(Index, _kind == AutoComplete.Next ? ++index : --index);
    }

    private int GetSuggestionIndex(LineBufferContext lineBufferContext, string word, string[] completions) {
        ArgumentNullException.ThrowIfNull(completions);

        if (string.IsNullOrWhiteSpace(word)) {
            return lineBufferContext.GetState(Index, () => 0).WrapAround(0, completions.Length - 1);
        }

        // Try to find an exact match
        var index = 0;
        foreach (var completion in completions) {
            if (completion.Equals(word, StringComparison.Ordinal)) {
                var newIndex = _kind == AutoComplete.Next ? index + 1 : index - 1;
                return newIndex.WrapAround(0, completions.Length - 1);
            }

            index++;
        }

        // Try to find a partial match
        index = 0;
        foreach (var completion in completions) {
            if (completion.StartsWith(word, StringComparison.Ordinal)) {
                return index;
            }

            index++;
        }

        return -1;
    }
}
