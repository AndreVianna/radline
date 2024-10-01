namespace RadLine.Commands.Cursor;

public sealed class MoveTextStartCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveTextStart);
}
