namespace RadLine.Commands.Cursor;

public sealed class MovePageDownCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MovePageDown);
}