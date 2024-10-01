namespace RadLine;

public sealed class KeyBindings {
    private readonly Dictionary<KeyBinding, Func<TextEditorCommand>> _bindings = new(new KeyBindingComparer());
    private readonly Dictionary<Type, KeyBinding> _bindingLookup = [];

    public int Count => _bindings.Count;

    internal void Add<TCommand>(KeyBinding binding, Func<TCommand> command)
        where TCommand : TextEditorCommand {
        ArgumentNullException.ThrowIfNull(binding);

        _bindings[binding] = command;
        _bindingLookup[typeof(TCommand)] = binding;
    }

    internal bool TryFindKeyBindings<TCommand>([NotNullWhen(true)] out KeyBinding? binding)
        where TCommand : TextEditorCommand => _bindingLookup.TryGetValue(typeof(TCommand), out binding);

    internal void Remove(KeyBinding binding) {
        ArgumentNullException.ThrowIfNull(binding);

        _bindings.Remove(binding);
    }

    public void Clear() => _bindings.Clear();

    public TextEditorCommand? GetCommand(ConsoleKey key, ConsoleModifiers? modifiers = null) {
        var candidates = _bindings.Keys as IEnumerable<KeyBinding>;

        if (modifiers is not null and not 0) {
            candidates = _bindings.Keys.Where(b => b.Modifiers == modifiers);
        }

        var result = candidates.FirstOrDefault(x => x.Key == key);
        return result != null && (modifiers != null || result.Modifiers == null) && _bindings.TryGetValue(result, out var factory)
            ? factory()
            : null;
    }
}
