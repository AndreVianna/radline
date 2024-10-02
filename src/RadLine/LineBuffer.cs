namespace RadLine;

public sealed class LineBuffer {
    public LineBuffer(string? content = null) {
        InitialContent = content ?? string.Empty;
        Content = InitialContent;
        Position = Content.Length;
    }

    public LineBuffer(LineBuffer buffer) {
        ArgumentNullException.ThrowIfNull(buffer);

        InitialContent = buffer.InitialContent;
        Content = buffer.Content;
        Position = Content.Length;
    }

    public int Position { get; private set; }

    public int Length => Content.Length;

    public string InitialContent { get; }

    public string Content { get; private set; }

    public bool IsAtBeginning => Position == 0;

    public bool IsAtEnd => Position == Content.Length;

    public (string start, string end) SplitAtCursor()
        => (Content[..Position], Content[Position..]);

    public bool IsAtCharacter
        => Length switch {
            0 => false,
            _ when IsAtEnd => false,
            _ => !char.IsWhiteSpace(Content[Position]),
        };

    public bool IsAtBeginningOfWord
        => Length switch {
            0 => false,
            _ when Position is 0 => !char.IsWhiteSpace(Content[0]),
            _ => char.IsWhiteSpace(Content[Position - 1]),
        };

    public bool IsAtEndOfWord
        => Length switch {
            0 => false,
            _ when Position is 0 => false,
            _ => !char.IsWhiteSpace(Content[Position - 1]),
        };

    // TODO: Right now, this only returns the position in the line buffer.
    // This is OK for western alphabets and most emojis which consist
    // of a single surrogate pair, but everything else will be wrong.

    public void SetPosition(int position) {
        var movingLeft = position < Position;
        Position = MoveToPosition(position, movingLeft);
    }

    public void Insert(char character) => Content = Content.Insert(Position, character.ToString());

    public void Insert(string text) => Content = Content.Insert(Position, text);

    public void Reset() {
        Content = InitialContent;
        Position = Content.Length;
    }

    public void Clear(int index, int count, bool moveBack = false) {
        if (index > Content.Length - 1) return;
        if (count <= 0) return;
        if (index < 0) index = 0;
        if (count > Content.Length - index) count = Content.Length - index;
        Content = Content.Remove(index, count);
        if (moveBack && Position > 0) Position--;
        if (Position > Content.Length) Position = Content.Length;
    }

    private int MoveToPosition(int position, bool movingLeft) {
        if (position <= 0) return 0;
        if (position >= Content.Length) return Content.Length;
        var indices = StringInfo.ParseCombiningCharacters(Content).Cast<int?>();

        return (movingLeft
            ? indices.Reverse().FirstOrDefault(e => e <= position)
            : indices.FirstOrDefault(e => e >= position))
               ?? throw new InvalidOperationException("Could not find position in buffer");
    }
}
