namespace RadLine.Prompts;

public sealed class LineNumberPromptPrefix(Style? style = null) : IPromptPrefix {
    private readonly Style _style = style ?? new Style(foreground: Color.Yellow, background: Color.Blue);

    public (Markup Markup, int Margin) GetPrompt(ITextEditorContent content, int line) => (new((line + 1).ToString("D2"), _style), 1);
}
