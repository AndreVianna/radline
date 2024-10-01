namespace RadLine.Internal.Extensions;

internal static class AnsiConsoleExtensions {
    public static IDisposable HideCursor(this IAnsiConsole console) => console switch {
        null => throw new ArgumentNullException(nameof(console)),
        _ => new CursorHider(console),
    };

    private sealed class CursorHider : IDisposable {
        private readonly IAnsiConsole _console;

        public CursorHider(IAnsiConsole console) {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _console.Cursor.Hide();
        }

        public void Dispose() => _console.Cursor.Show();
    }
}
