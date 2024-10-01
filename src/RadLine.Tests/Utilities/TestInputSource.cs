namespace RadLine.Utilities;

public sealed class TestInputSource : IInputSource
{
    private readonly Queue<ConsoleKeyInfo> _input;

    public bool ByPassProcessing => true;

    public TestInputSource()
    {
        _input = new();
    }

    public TestInputSource Push(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        foreach (var character in input)
        {
            Push(character);
        }

        return this;
    }

    public TestInputSource Push(char input)
    {
        var control = char.IsUpper(input);
        _input.Enqueue(new(input, (ConsoleKey)input, false, false, control));
        return this;
    }

    public TestInputSource Push(ConsoleKey input)
    {
        _input.Enqueue(new((char)input, input, false, false, false));
        return this;
    }

    public TestInputSource PushNewLine()
    {
        Push(ConsoleKey.Enter, ConsoleModifiers.Shift);
        return this;
    }

    public TestInputSource PushSubmit()
    {
        Push(ConsoleKey.Enter);
        return this;
    }

    public TestInputSource PushEscape()
    {
        Push(ConsoleKey.Escape);
        return this;
    }

    public TestInputSource Push(ConsoleKey input, ConsoleModifiers modifiers)
    {
        var shift = modifiers.HasFlag(ConsoleModifiers.Shift);
        var control = modifiers.HasFlag(ConsoleModifiers.Control);
        var alt = modifiers.HasFlag(ConsoleModifiers.Alt);

        _input.Enqueue(new((char)0, input, shift, alt, control));
        return this;
    }

    public bool IsKeyAvailable() => _input.Count > 0;

    ConsoleKeyInfo IInputSource.ReadKey() => _input.Count switch
    {
        0 => throw new InvalidOperationException("No keys available"),
        _ => _input.Dequeue(),
    };
}
