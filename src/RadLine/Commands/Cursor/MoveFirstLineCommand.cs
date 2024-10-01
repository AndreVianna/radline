namespace RadLine.Commands.Cursor;

public sealed class MoveFirstLineCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveTop);
}
