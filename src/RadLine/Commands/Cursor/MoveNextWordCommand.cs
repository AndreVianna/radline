namespace RadLine.Commands.Cursor;

public sealed class MoveNextWordCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.MoveToNextWord);
}
