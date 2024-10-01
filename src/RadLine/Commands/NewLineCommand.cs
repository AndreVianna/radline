namespace RadLine.Commands;

public sealed class NewLineCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.NewLine);
}
