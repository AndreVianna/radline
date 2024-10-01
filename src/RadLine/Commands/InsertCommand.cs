namespace RadLine.Commands;

public sealed class InsertCommand : TextEditorCommand {
    private readonly char? _character;
    private readonly string? _text;

    public InsertCommand(char character) {
        _character = character;
        _text = null;
    }

    public InsertCommand(string text) {
        _text = text ?? string.Empty;
        _character = null;
    }

    public override void Execute(LineBufferContext lineBufferContext) {
        var buffer = lineBufferContext.Buffer;

        if (_character != null) {
            buffer.Insert(_character.Value);
            buffer.SetPosition(buffer.Position + 1);
        }
        else if (_text != null) {
            buffer.Insert(_text);
            buffer.SetPosition(buffer.Position + _text.Length);
        }
    }
}
