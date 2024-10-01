namespace RadLine;

public static class KeyBindingsExtensions {
    public static void AddDefault(this KeyBindings bindings) {
        bindings.Add(ConsoleKey.Tab, () => new AutoCompleteCommand(AutoComplete.Next));
        bindings.Add(ConsoleKey.Tab, ConsoleModifiers.Shift, () => new AutoCompleteCommand(AutoComplete.Previous));

        bindings.Add<BackspaceCommand>(ConsoleKey.Backspace);
        bindings.Add<DeleteCommand>(ConsoleKey.Delete);

        bindings.Add<MoveLeftCommand>(ConsoleKey.LeftArrow);
        bindings.Add<MoveRightCommand>(ConsoleKey.RightArrow);
        bindings.Add<MoveLineStartCommand>(ConsoleKey.Home);
        bindings.Add<MoveLineEndCommand>(ConsoleKey.End);
        bindings.Add<MovePreviousWordCommand>(ConsoleKey.LeftArrow, ConsoleModifiers.Control);
        bindings.Add<MoveNextWordCommand>(ConsoleKey.RightArrow, ConsoleModifiers.Control);

        bindings.Add<MoveUpCommand>(ConsoleKey.UpArrow);
        bindings.Add<MoveDownCommand>(ConsoleKey.DownArrow);
        bindings.Add<MovePageUpCommand>(ConsoleKey.PageUp);
        bindings.Add<MovePageDownCommand>(ConsoleKey.PageDown);
        bindings.Add<MoveFirstLineCommand>(ConsoleKey.PageUp, ConsoleModifiers.Control);
        bindings.Add<MoveLastLineCommand>(ConsoleKey.PageDown, ConsoleModifiers.Control);
        bindings.Add<MoveTextStartCommand>(ConsoleKey.Home, ConsoleModifiers.Control);
        bindings.Add<MoveTextEndCommand>(ConsoleKey.End, ConsoleModifiers.Control);

        bindings.Add<PreviousHistoryCommand>(ConsoleKey.UpArrow, ConsoleModifiers.Control);
        bindings.Add<NextHistoryCommand>(ConsoleKey.DownArrow, ConsoleModifiers.Control);

        bindings.Add<CancelCommand>(ConsoleKey.Escape);
        bindings.Add<SubmitCommand>(ConsoleKey.Enter);
        bindings.Add<NewLineCommand>(ConsoleKey.Enter, ConsoleModifiers.Shift);
    }

    public static void Add<TCommand>(this KeyBindings bindings, ConsoleKey key, ConsoleModifiers? modifiers = null)
        where TCommand : TextEditorCommand, new() {
        ArgumentNullException.ThrowIfNull(bindings);

        bindings.Add(new(key, modifiers), () => new TCommand());
    }

    public static void Add<TCommand>(this KeyBindings bindings, ConsoleKey key, Func<TCommand> func)
        where TCommand : TextEditorCommand {
        ArgumentNullException.ThrowIfNull(bindings);

        bindings.Add(new(key), () => func());
    }

    public static void Add<TCommand>(this KeyBindings bindings, ConsoleKey key, ConsoleModifiers modifiers, Func<TCommand> func)
        where TCommand : TextEditorCommand {
        ArgumentNullException.ThrowIfNull(bindings);

        bindings.Add(new(key, modifiers), () => func());
    }

    public static void Remove(this KeyBindings bindings, ConsoleKey key, ConsoleModifiers? modifiers = null) {
        ArgumentNullException.ThrowIfNull(bindings);

        bindings.Remove(new(key, modifiers));
    }
}
