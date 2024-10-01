namespace RadLine;

internal sealed class TextEditorHistory : ITextEditorHistory {
    private readonly LinkedList<LineBuffer[]> _history = [];
    private LinkedListNode<LineBuffer[]>? _current;
    private LineBuffer[]? _intermediate;
    private bool _showIntermediate;

    public int Count => _history.Count;

    public LineBuffer[]? Current
        => _showIntermediate && _intermediate != null
            ? _intermediate
            : _current?.Value;

    public void Add(string text) {
        ArgumentNullException.ThrowIfNull(text);

        var lines = text.NormalizeNewLines().Split(['\n']);
        var buffers = new LineBuffer[lines.Length];
        for (var index = 0; index < lines.Length; index++) {
            buffers[index] = new(lines[index]);
        }

        _history.AddLast(buffers);
    }

    internal void Reset() {
        _current = null;
        _intermediate = null;
    }

    internal void Add(IList<LineBuffer> buffers) {
        ArgumentNullException.ThrowIfNull(buffers);

        var shouldAdd = _history.Last == null;
        if (_history.Last != null) {
            if (_history.Last.Value.Length != buffers.Count) {
                // Not the same length so not the same content.
                shouldAdd = true;
            }
            else {
                // Compare the buffers line by line
                for (var index = 0; index < buffers.Count; index++) {
                    // Not the same content?
                    if (!buffers[index].Content.Equals(_history.Last.Value[index].Content, StringComparison.Ordinal)) {
                        shouldAdd = true;
                        break;
                    }
                }
            }
        }

        if (shouldAdd) {
            _history.AddLast(buffers as LineBuffer[] ?? [.. buffers]);
        }

        _current = null;
    }

    internal bool MovePrevious(TextEditorContent content) {
        // At the last one?
        if ((_current == null && _intermediate == null) || _showIntermediate) {
            _intermediate = string.IsNullOrWhiteSpace(content.Text) switch {
                // Got something written that we don't want to lose?
                false => [.. content.GetBuffers()],
                _ => _intermediate,
            };
        }

        _showIntermediate = false;

        if (_current == null && _history.Count > 0) {
            _current = _history.Last;
            return true;
        }

        if (_current?.Previous != null) {
            _current = _current.Previous;
            return true;
        }

        return false;
    }

    internal bool MoveNext() {
        if (_current == null) {
            return false;
        }

        if (_current?.Next != null) {
            _current = _current.Next;
            return true;
        }

        // Got an intermediate buffer to show?
        if (_intermediate != null) {
            _showIntermediate = true;
            return true;
        }

        return false;
    }
}
