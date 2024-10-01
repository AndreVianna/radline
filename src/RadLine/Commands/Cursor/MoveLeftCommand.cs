namespace RadLine.Commands.Cursor;

public sealed class MoveLeftCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveLeft);
}
