namespace RadLine.Commands;

public sealed class DeleteCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.Delete);
    //public override void Execute(LineBufferContext lineBufferContext) {
    //    var buffer = lineBufferContext.Buffer;
    //    buffer.Clear(buffer.Position, 1);
    //}
}
