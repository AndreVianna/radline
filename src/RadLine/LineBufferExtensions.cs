namespace RadLine;

public static class LineBufferExtensions {
    public static bool MoveLeft(this LineBuffer buffer, int count = 1) => buffer.SetPosition(buffer.Position - count);

    public static bool MoveRight(this LineBuffer buffer, int count = 1) => buffer.SetPosition(buffer.Position + count);

    public static bool MoveLineStart(this LineBuffer buffer) => buffer.SetPosition(0);

    public static bool MoveLineEnd(this LineBuffer buffer) => buffer.SetPosition(buffer.Length);

    public static bool MoveToPreviousWord(this LineBuffer buffer) {
        var position = buffer.Position;
        if (buffer is { IsAtCharacter: true, IsAtBeginningOfWord: true }) buffer.MoveLeft();
        while (buffer is { Position: > 0, IsAtCharacter: false }) buffer.MoveLeft();
        buffer.MoveToBeginningOfWord();
        return position != buffer.Position;
    }

    public static bool MoveToNextWord(this LineBuffer buffer) {
        var position = buffer.Position;
        if (buffer.IsAtCharacter) buffer.MoveToEndOfWord();
        while (buffer is { AtEnd: false, IsAtCharacter: false }) buffer.MoveRight();
        return position != buffer.Position;
    }

    public static bool MoveToEndOfWord(this LineBuffer buffer) {
        if (buffer.AtEnd || !buffer.IsAtCharacter) return false;
        var position = buffer.Position;
        while (buffer is { AtEnd: false, IsAtCharacter: true }) buffer.MoveRight();
        return position != buffer.Position;
    }

    public static bool MoveToBeginningOfWord(this LineBuffer buffer) {
        if (buffer.Position == 0 || !buffer.IsAtCharacter) return false;
        var position = buffer.Position;
        while (buffer.Position > 0) {
            if (char.IsWhiteSpace(buffer.Content[buffer.Position - 1])) break;
            buffer.MoveLeft();
        }
        return position != buffer.Position;
    }
}
