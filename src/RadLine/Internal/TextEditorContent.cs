namespace RadLine.Internal;

internal sealed class TextEditorContent : ITextEditorContent {
    public TextEditorContent(IPromptPrefix promptPrefix, string text) {
        _lines = [];
        PromptPrefix = promptPrefix ?? throw new ArgumentNullException(nameof(promptPrefix));
        SetContent(text.NormalizeNewLines().Split(['\n']).Select(l => new LineBuffer(l)), 0);
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

    private void SetContent(IEnumerable<LineBuffer> buffers, int lineIndex) {
        ArgumentNullException.ThrowIfNull(buffers);
        _lines.Clear();
        _lines.AddRange(buffers);
        if (_lines.Count == 0) _lines.Add(new());
        LineIndex = lineIndex;
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

    public void Clear() {
        _lines.Clear();
        LineIndex = 0;
    }

    public void SetContent(ICollection<string?>? content = null) {
        var line = LineIndex;
        var position = Buffer.Position;
        Clear();
        AppendLines(content ?? [string.Empty]);
        if (line < LineCount) Move(line);
        if (position < Buffer.Length) Buffer.SetPosition(position);
    }

    public void AppendLines(ICollection<string?> content) {
        _lines.AddRange(content.Select(l => new LineBuffer(l)));
        LineIndex += content.Count;
    }

    public void AppendLine(string? content = null) {
        _lines.Add(new(content));
        LineIndex++;
    }

    public void InsertLines(int index, ICollection<string?> content) {
        if (index < 0) index = 0;
        if (index > _lines.Count) index = _lines.Count;
        _lines.InsertRange(index, content.Select(l => new LineBuffer(l)));
        if (index <= LineIndex) LineIndex += content.Count;
    }

    public void InsertLine(int index, string? content = null) {
        if (index < 0) index = 0;
        if (index > _lines.Count) index = _lines.Count;
        _lines.Insert(index, new(content));
        if (index <= LineIndex) LineIndex++;
    }

    public void RemoveLine(int index) {
        _lines.RemoveAt(index);
        if (index <= LineIndex) LineIndex--;
    }
}
