namespace RadLine.Commands.Cursor;

public sealed class MovePreviousWordCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveToPreviousWord);
}
