namespace RadLine.Commands;

public sealed class BackspaceCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.Backspace);
    //public override void Execute(LineBufferContext lineBufferContext) {
    //    var removed = lineBufferContext.Buffer.Clear(lineBufferContext.Buffer.Position - 1, 1);
    //    if (removed == 1) {
    //        lineBufferContext.Buffer.SetPosition(lineBufferContext.Buffer.Position - 1);
    //    }
    //}
}
