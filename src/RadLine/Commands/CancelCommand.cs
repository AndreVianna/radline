namespace RadLine.Commands;

public sealed class CancelCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) {
        lineBufferContext.Buffer.Reset();
        lineBufferContext.Submit(SubmitAction.Cancel);
    }
}
