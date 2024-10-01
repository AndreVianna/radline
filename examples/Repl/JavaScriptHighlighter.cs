namespace RadLine.Examples;

public sealed class JavaScriptHighlighter : IHighlighter {
    private readonly IHighlighter _highlighter = CreateHighlighter();

    private static readonly string[] _keywords =
    [
        "await", "break", "case", "catch", "class",
        "const", "continue", "debugger", "default",
        "delete", "do", "else", "enum", "export",
        "extends", "false", "finally", "for", "function",
        "if", "implements", "import", "in", "instanceof",
        "interface", "let", "new", "null", "package",
        "private", "protected", "public", "return",
        "super", "switch", "static", "this", "throw",
        "try", "true", "typeof", "var", "void", "while",
        "with", "yield"
    ];

    private static WordHighlighter CreateHighlighter() {
        var highlighter = new WordHighlighter();
        foreach (var keyword in _keywords) {
            highlighter.AddWord(keyword, new(foreground: Color.Blue));
        }

        highlighter.AddWord("{", new(foreground: Color.Grey));
        highlighter.AddWord("}", new(foreground: Color.Grey));
        highlighter.AddWord("(", new(foreground: Color.Grey));
        highlighter.AddWord(")", new(foreground: Color.Grey));
        highlighter.AddWord(";", new(foreground: Color.Grey));

        return highlighter;
    }
    public IRenderable BuildHighlightedText(string text)
        => _highlighter.BuildHighlightedText(text);
}