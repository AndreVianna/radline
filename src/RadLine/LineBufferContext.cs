namespace RadLine;

public sealed class LineBufferContext(LineBuffer buffer, IServiceProvider? provider = null)
    : IServiceProvider {
    private readonly Dictionary<string, object?> _state = new(StringComparer.OrdinalIgnoreCase);

    public LineBuffer Buffer { get; } = buffer ?? throw new ArgumentNullException(nameof(buffer));

    internal SubmitAction? Result { get; private set; }

    public object? GetService(Type serviceType) => provider?.GetService(serviceType);

    public void Execute(TextEditorCommand command) {
        if (Result != null) return;
        command.Execute(this);
    }

    public void SetState<T>(string key, T value) => _state[key] = value;

    public T GetState<T>(string key, Func<T> defaultValue)
        => _state.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue
            : defaultValue();

    public void ClearResult() => Result = null;
    public void Submit(SubmitAction action) => Result = action;
}
