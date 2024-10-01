namespace RadLine.Commands.Cursor;

public sealed class MovePageUpCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MovePageUp);
}