namespace RadLine.Commands.Cursor;

public sealed class MoveRightCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveRight);
}
