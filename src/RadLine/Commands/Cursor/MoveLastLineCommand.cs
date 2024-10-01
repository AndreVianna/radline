namespace RadLine.Commands.Cursor;

public sealed class MoveLastLineCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveBottom);
}
