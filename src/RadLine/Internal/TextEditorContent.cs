namespace RadLine.Internal;

internal sealed class TextEditorContent : ITextEditorContent {
    public TextEditorContent(IPromptPrefix promptPrefix, string text) {
        _lines = [];
        LineIndex = 0;

        PromptPrefix = promptPrefix ?? throw new ArgumentNullException(nameof(promptPrefix));

        // Add all lines
        foreach (var line in text.NormalizeNewLines().Split(['\n'])) {
            _lines.Add(new(line));
        }

        // No lines?
        if (_lines.Count == 0) {
            _lines.Add(new());
        }
    }

    private readonly List<LineBuffer> _lines;

    public IPromptPrefix PromptPrefix { get; }

    public int LineIndex { get; private set; }

    public int LineCount => _lines.Count;

    public bool IsFirstLine => LineIndex == 0;

    public bool IsLastLine => LineIndex == _lines.Count - 1;

    public LineBuffer Buffer => _lines[LineIndex];

    public bool IsEmpty => string.IsNullOrWhiteSpace(Text.TrimEnd('\r', '\n'));

    public string Text => string.Join(Environment.NewLine, _lines.Select(x => x.Content));

    public LineBuffer GetBufferAt(int line) => _lines[line];

    public IList<LineBuffer> GetBuffers() => _lines;

    public bool SetContent(IList<LineBuffer> buffers, int lineIndex) {
        ArgumentNullException.ThrowIfNull(buffers);

        if (buffers.Count == 0) {
            return false;
        }

        _lines.Clear();
        _lines.AddRange(buffers);
        LineIndex = lineIndex;

        return true;
    }

    public void Move(int line) => LineIndex = Math.Max(0, Math.Min(line, LineCount - 1));

    public bool MoveUp() {
        if (LineIndex <= 0) return false;
        LineIndex--;
        return true;
    }

    public bool MoveDown() {
        if (LineIndex >= _lines.Count - 1) return false;
        LineIndex++;
        return true;
    }

    public void RemoveAllLines() {
        _lines.Clear();
        LineIndex = -1;
    }

    public void AddLine(string? content = null) {
        _lines.Add(new(content));
        LineIndex++;
    }
}
