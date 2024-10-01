namespace RadLine.Examples;

public sealed class InsertSmiley : TextEditorCommand {
    public override void Execute(LineBufferContext lineBufferContext)
        => lineBufferContext.Buffer.Insert(":-)");
}