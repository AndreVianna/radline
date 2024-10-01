namespace RadLine.Internal;

internal sealed class KeyBindingComparer : IEqualityComparer<KeyBinding> {
    public bool Equals(KeyBinding? x, KeyBinding? y) => (x == null && y == null) || (x != null && y != null && x.Key == y.Key && x.Modifiers == y.Modifiers);

    public int GetHashCode(KeyBinding obj) {
#if NET5_0
            return HashCode.Combine(obj.Key, obj.Modifiers);
#else
        unchecked {
            var hash = 17;
            hash = (hash * 23) + (int)obj.Key;
            hash = (hash * 23) + (obj.Modifiers != null ? (int)obj.Modifiers : 0);
            return hash;
        }
#endif
    }
}
