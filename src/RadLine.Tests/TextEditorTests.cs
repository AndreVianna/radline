namespace RadLine;

public sealed class TextEditorTests {
    [Fact]
    public async Task Should_Return_Original_Text_When_Pressing_Escape() {
        // Given
        var editor = new TextEditor(
            new TestConsole(),
            new TestInputSource()
                .Push("Bar")
                .PushEscape()) {
            Text = "Foo",
        };

        // When
        var result = await editor.ReadText(CancellationToken.None);

        // Then
        result.ShouldBe("Foo");
    }

    [Fact]
    public async Task Should_Return_Entered_Text_When_Pressing_Enter() {
        // Given
        var editor = new TextEditor(
            new TestConsole(),
            new TestInputSource()
                .Push("Patrik")
                .PushSubmit());

        // When
        var result = await editor.ReadText(CancellationToken.None);

        // Then
        result.ShouldBe("Patrik");
    }

    [Fact]
    public async Task Should_Add_New_Line_When_Pressing_Shift_Enter() {
        // Given
        var editor = new TextEditor(
            new TestConsole(),
            new TestInputSource()
                .Push("Patrik")
                .PushNewLine()
                .Push("Svensson")
                .PushSubmit());

        // When
        var result = await editor.ReadText(CancellationToken.None);

        // Then
        result.ShouldBe($"Patrik{Environment.NewLine}Svensson");
    }

    [Fact]
    public async Task Should_Move_To_Previous_Item_In_History() {
        // Given
        var editor = new TextEditor(
            new TestConsole(),
            new TestInputSource()
                .Push(ConsoleKey.UpArrow, ConsoleModifiers.Control)
                .Push(ConsoleKey.UpArrow, ConsoleModifiers.Control)
                .Push(ConsoleKey.UpArrow, ConsoleModifiers.Control)
                .PushSubmit());

        editor.History.Add("Foo");
        editor.History.Add("Bar");
        editor.History.Add("Baz");

        // When
        var result = await editor.ReadText(CancellationToken.None);

        // Then
        result.ShouldBe("Foo");
    }

    [Fact]
    public async Task Should_Move_To_Next_Item_In_History() {
        // Given
        var editor = new TextEditor(
            new TestConsole(),
            new TestInputSource()
                .Push(ConsoleKey.UpArrow, ConsoleModifiers.Control)
                .Push(ConsoleKey.UpArrow, ConsoleModifiers.Control)
                .Push(ConsoleKey.UpArrow, ConsoleModifiers.Control)
                .Push(ConsoleKey.DownArrow, ConsoleModifiers.Control)
                .Push(ConsoleKey.DownArrow, ConsoleModifiers.Control)
                .PushSubmit());

        editor.History.Add("Foo");
        editor.History.Add("Bar");
        editor.History.Add("Baz");

        // When
        var result = await editor.ReadText(CancellationToken.None);

        // Then
        result.ShouldBe("Baz");
    }

    [Fact]
    public async Task Should_Add_Entered_Text_To_History() {
        // Given
        var input = new TestInputSource();
        var editor = new TextEditor(new TestConsole(), input);
        input.Push("Patrik").PushSubmit();
        await editor.ReadText(CancellationToken.None);

        // When
        input.Push(ConsoleKey.UpArrow, ConsoleModifiers.Control).PushSubmit();
        var result = await editor.ReadText(CancellationToken.None);

        // Then
        result.ShouldBe("Patrik");
    }

    [Fact]
    public async Task Should_Not_Add_Entered_Text_To_History_If_Its_The_Same_As_The_Last_Entry() {
        // Given
        var input = new TestInputSource();
        var editor = new TextEditor(new TestConsole(), input);
        input.Push("Patrik").PushNewLine().Push("Svensson").PushSubmit();
        await editor.ReadText(CancellationToken.None);

        // When
        input.Push("Patrik").PushNewLine().Push("Svensson").PushSubmit();
        var result = await editor.ReadText(CancellationToken.None);

        // Then
        result.ShouldBe($"Patrik{Environment.NewLine}Svensson");
        editor.History.Count.ShouldBe(1);
    }
}
