namespace RadLine.Internal;

internal sealed class DefaultInputSource(IAnsiConsole console) : IInputSource {
    private readonly IAnsiConsole _console = console ?? throw new ArgumentNullException(nameof(console));

    public bool ByPassProcessing => false;

    public bool IsKeyAvailable() => Console.KeyAvailable;

    public ConsoleKeyInfo ReadKey() {
        if (!_console.Profile.Out.IsTerminal || !_console.Profile.Capabilities.Interactive) {
            throw new NotSupportedException("Only interactive terminals are supported as input source");
        }

        // TODO: Put terminal in raw mode
        return Console.ReadKey(true);
    }
}
