if (!TextEditor.IsSupported(AnsiConsole.Console)) {
    AnsiConsole.MarkupLine("The terminal does not support ANSI codes, or it isn't a terminal.");
}

// Create editor
var editor = new TextEditor() {
    Text = "Hello, and welcome to RadLine!\nPress SHIFT+ENTER to insert a new line\nPress ENTER to submit",
    PromptPrefix = new LineNumberPromptPrefix(new(foreground: Color.Yellow, background: Color.Black)),
    //    MaximumNumberOfLines = 5,
    Completion = new TestCompletion(),
    Highlighter = new WordHighlighter()
        .AddWord("git", new(foreground: Color.Yellow))
        .AddWord("code", new(foreground: Color.Yellow))
        .AddWord("vim", new(foreground: Color.Yellow))
        .AddWord("init", new(foreground: Color.Blue))
        .AddWord("push", new(foreground: Color.Red))
        .AddWord("commit", new(foreground: Color.Blue))
        .AddWord("rebase", new(foreground: Color.Red))
        .AddWord("Hello", new(foreground: Color.Blue))
        .AddWord("SHIFT", new(foreground: Color.Grey))
        .AddWord("ENTER", new(foreground: Color.Grey))
        .AddWord("RadLine", new(foreground: Color.Yellow, decoration: Decoration.SlowBlink)),
};

// Add some history
editor.History.Add("foo\nbar\nbaz");
editor.History.Add("bar");
editor.History.Add("Spectre.Console");
editor.History.Add("Whaaat?");

// Add custom commands
editor.KeyBindings.Add<InsertSmiley>(ConsoleKey.I, ConsoleModifiers.Control);

var result = string.Empty;
while (editor.State is TextEditorState.Active) {
    result = await editor.ReadText(CancellationToken.None);
    if (editor.State != TextEditorState.Invalid) continue;
    AnsiConsole.WriteLine(editor.ErrorMessage);
    AnsiConsole.WriteLine("[yellow]Please try again.[/]");
    AnsiConsole.WriteLine();
    editor.ResetState();
}

// Write the buffer
AnsiConsole.WriteLine();
AnsiConsole.Write(new Panel(result.EscapeMarkup())
    .Header("[yellow]Text:[/]")
    .RoundedBorder());
