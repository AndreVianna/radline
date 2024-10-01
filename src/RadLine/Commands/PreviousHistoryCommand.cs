namespace RadLine.Commands;

public sealed class PreviousHistoryCommand : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext) => lineBufferContext.Submit(SubmitAction.PreviousHistory);
}
