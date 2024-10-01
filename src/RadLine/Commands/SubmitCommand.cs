namespace RadLine.Commands;

public sealed class SubmitCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.Submit);
}
