namespace RadLine.Examples;

public static class Program {
    public static async Task Main() {
        if (!TextEditor.IsSupported(AnsiConsole.Console)) {
            AnsiConsole.MarkupLine("The terminal does not support ANSI codes, or it isn't a terminal.");
        }

        AnsiConsole.Write(new FigletText("JS REPL"));
        AnsiConsole.MarkupLine("Type [yellow]exit[/] to leave, " +
                               "[yellow]print([grey]obj[/])[/] to write to the console, " +
                               "[yellow]load([grey]'path'[/])[/] to load scripts.");

        // Create editor
        var editor = new TextEditor() {
            PromptPrefix = new DefaultPromptPrefix(">", "."),
            Highlighter = new JavaScriptHighlighter(),
        };

        // Create the JavaScript engine
        var engine = new JavaScriptEngine(cfg => cfg.AllowClr());
        engine.SetValue("print", new Action<object>(res => AnsiConsole.MarkupLine("[yellow]" + res.ToString().EscapeMarkup() + "[/]")));
        engine.SetValue("load", new Func<string, object>(path => engine.Evaluate(File.ReadAllText(path))));

        while (true) {
            AnsiConsole.WriteLine();
            var source = await editor.ReadText(CancellationToken.None);
            if (source.Equals("exit", StringComparison.OrdinalIgnoreCase)) {
                break;
            }
            else if (source.Equals("clear", StringComparison.OrdinalIgnoreCase)) {
                AnsiConsole.Console.Clear(true);
                continue;
            }

            Evaluate(engine, source);
        }
    }

    private static void Evaluate(JavaScriptEngine engine, string source) => AnsiConsole.Status()
            .Start("Evaluating...", _ => {
                try {
                    var result = engine.Evaluate(source);
                    if (!result.IsNull() && !result.IsUndefined()) {
                        var stringResult = TypeConverter.ToString(engine.Json.Stringify(engine.Json, Arguments.From(result, Undefined.Instance, "  ")));
                        Print(stringResult, Color.Green);
                    }
                }
                catch (JavaScriptException je) {
                    Print(je.ToString(), Color.Red);
                }
                catch (Exception e) {
                    Print(e.Message, Color.Red);
                }
            });

    private static void Print(string message, Color color) => AnsiConsole.Write(
            new Padder(
                new Panel(message).BorderColor(color),
                new Padding(2, 0)));
}
