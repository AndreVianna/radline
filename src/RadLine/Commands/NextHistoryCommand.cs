namespace RadLine.Commands;

public sealed class NextHistoryCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.NextHistory);
}
