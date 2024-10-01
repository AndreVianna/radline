namespace RadLine.Internal;

internal sealed class KeyBinding(ConsoleKey key, ConsoleModifiers? modifiers = null) : IEquatable<ConsoleKeyInfo> {
    public ConsoleKey Key { get; } = key;

    public ConsoleModifiers? Modifiers { get; } = modifiers;

    public ConsoleKeyInfo AsConsoleKeyInfo() => new(
            (char)0, Key,
            HasModifier(ConsoleModifiers.Shift),
            HasModifier(ConsoleModifiers.Alt),
            HasModifier(ConsoleModifiers.Control));

    private bool HasModifier(ConsoleModifiers modifier) => Modifiers?.HasFlag(modifier) ?? false;

    public bool Equals(ConsoleKeyInfo other) => other.Modifiers == (Modifiers ?? 0) && other.Key == Key;
}
