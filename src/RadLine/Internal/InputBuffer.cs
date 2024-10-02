namespace RadLine.Internal;

internal sealed class InputBuffer(IInputSource source) {
    private readonly IInputSource _source = source ?? throw new ArgumentNullException(nameof(source));
    private readonly Queue<ConsoleKeyInfo> _queue = [];
    private KeyBinding? _newLineBinding;
    private KeyBinding? _submitBinding;

    public void Initialize(KeyBindings bindings) {
        bindings.TryFindKeyBindings<NewLineCommand>(out _newLineBinding);
        bindings.TryFindKeyBindings<SubmitCommand>(out _submitBinding);
    }

    public async Task<ConsoleKeyInfo?> ReadKey(bool multiline, CancellationToken cancellationToken) {
        if (_queue.Count > 0) return _queue.Dequeue();
        var key = await ReadKeyFromSource(wait: true, cancellationToken);
        if (key == null) return null;
        _queue.Enqueue(key.Value);
        if (_source.IsKeyAvailable()) await ReadRemainingKeys(multiline, cancellationToken);

        return _queue.Count switch {
            > 0 => _queue.Dequeue(),
            _ => null,
        };
    }

    private async Task ReadRemainingKeys(bool multiline, CancellationToken cancellationToken) {
        var keys = new Queue<ConsoleKeyInfo>();

        while (true) {
            var key = await ReadKeyFromSource(wait: false, cancellationToken);
            if (key == null) break;
            keys.Enqueue(key.Value);
        }

        if (keys.Count == 0) return;
        var shouldProcess = multiline && keys.Count >= 5 && !_source.ByPassProcessing;
        while (keys.Count > 0) {
            var key = keys.Dequeue();
            if (shouldProcess && _submitBinding != null && _newLineBinding != null && _submitBinding.Equals(key))
                key = _newLineBinding.AsConsoleKeyInfo();
            _queue.Enqueue(key);
        }
    }

    private async Task<ConsoleKeyInfo?> ReadKeyFromSource(bool wait, CancellationToken cancellationToken) {
        if (!wait) return _source.IsKeyAvailable() ? _source.ReadKey() : null;
        while (true) {
            if (cancellationToken.IsCancellationRequested) return null;
            if (_source.IsKeyAvailable()) break;
            await Task.Delay(5, cancellationToken).ConfigureAwait(false);
        }

        return _source.IsKeyAvailable() ? _source.ReadKey() : null;
    }
}
