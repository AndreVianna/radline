namespace RadLine.Commands.Cursor;

public sealed class MoveDownCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveDown);
}
