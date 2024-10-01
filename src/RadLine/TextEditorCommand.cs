namespace RadLine;

public abstract class TextEditorCommand {
    public abstract void Execute(LineBufferContext lineBufferContext);
}
