namespace RadLine;

public static class LineBufferExtensions {
    public static void MoveLeft(this LineBuffer buffer, int count = 1) => buffer.SetPosition(buffer.Position - count);

    public static void MoveRight(this LineBuffer buffer, int count = 1) => buffer.SetPosition(buffer.Position + count);

    public static void MoveLineStart(this LineBuffer buffer) => buffer.SetPosition(0);

    public static void MoveLineEnd(this LineBuffer buffer) => buffer.SetPosition(buffer.Length);

    public static void MoveToPreviousWord(this LineBuffer buffer) {
        if (buffer is { IsAtCharacter: true, IsAtBeginningOfWord: true }) buffer.MoveLeft();
        while (buffer is { Position: > 0, IsAtCharacter: false }) buffer.MoveLeft();
        buffer.MoveToBeginningOfWord();
    }

    public static void MoveToNextWord(this LineBuffer buffer) {
        if (buffer.IsAtCharacter) buffer.MoveToEndOfWord();
        while (buffer is { IsAtEnd: false, IsAtCharacter: false }) buffer.MoveRight();
    }

    public static void MoveToEndOfWord(this LineBuffer buffer) {
        if (buffer.IsAtEnd || !buffer.IsAtCharacter) return;
        while (buffer is { IsAtEnd: false, IsAtCharacter: true }) buffer.MoveRight();
    }

    public static void MoveToBeginningOfWord(this LineBuffer buffer) {
        if (buffer.Position == 0 || !buffer.IsAtCharacter) return;
        while (buffer.Position > 0) {
            if (char.IsWhiteSpace(buffer.Content[buffer.Position - 1])) break;
            buffer.MoveLeft();
        }
    }
}
