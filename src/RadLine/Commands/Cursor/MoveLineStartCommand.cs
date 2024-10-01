namespace RadLine.Commands.Cursor;

public sealed class MoveLineStartCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveLineStart);
}
