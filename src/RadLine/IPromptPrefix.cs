namespace RadLine;

public interface IPromptPrefix {
    (Markup Markup, int Margin) GetPrompt(ITextEditorContent content, int line);
}
