namespace RadLine;

public sealed class WordHighlighter(StringComparer? comparer = null) : IHighlighter {
    private readonly Dictionary<string, Style> _words = new(comparer ?? StringComparer.OrdinalIgnoreCase);

    public WordHighlighter AddWord(string word, Style style) {
        _words[word] = style;
        return this;
    }

    IRenderable IHighlighter.BuildHighlightedText(string text) {
        var paragraph = new Paragraph();
        foreach (var token in StringTokenizer.Tokenize(text)) {
            if (_words.TryGetValue(token, out var style)) {
                paragraph.Append(token, style);
            }
            else {
                paragraph.Append(token, null);
            }
        }

        return paragraph;
    }
}
