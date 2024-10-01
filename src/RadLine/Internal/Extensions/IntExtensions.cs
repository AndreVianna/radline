namespace RadLine.Internal.Extensions;

internal static class IntExtensions {
    public static int Clamp(this int value, int min, int max) => value < min ? min : value > max ? max : value;

    public static int WrapAround(this int value, int min, int max) => value < min ? max : value > max ? min : value;
}
