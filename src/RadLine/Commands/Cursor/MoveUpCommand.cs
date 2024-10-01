namespace RadLine.Commands.Cursor;

public sealed class MoveUpCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveUp);
}
