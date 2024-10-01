namespace RadLine.Commands.Cursor;

public sealed class MoveLineEndCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveLineEnd);
}
