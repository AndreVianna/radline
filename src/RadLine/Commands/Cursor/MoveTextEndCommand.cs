namespace RadLine.Commands.Cursor;

public sealed class MoveTextEndCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveTextEnd);
}
