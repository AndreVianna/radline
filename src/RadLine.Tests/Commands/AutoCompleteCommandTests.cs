namespace RadLine.Commands;

public sealed class AutoCompleteCommandTests {
    private sealed class Fixture(string? text = null) {
        private LineBufferContext? _context;

        public LineBuffer Buffer { get; } = new(text);

        public Func<string, string, string, IEnumerable<string>?> AutoComplete { get; set; } = (_, _, _) => null;

        public void Execute(AutoComplete direction) {
            var provider = new SimpleServiceProvider();
            provider.Register<ITextCompletion, DelegateTextCompletion>(new(AutoComplete));

            _context ??= new(Buffer, provider);
            var command = new AutoCompleteCommand(direction);
            command.Execute(_context);
        }
    }

    public sealed class Next {
        [Fact]
        public void Should_Insert_First_AutoComplete_Suggestion() {
            // Given
            var fixture = new Fixture();
            fixture.Buffer.Insert("git ");
            fixture.Buffer.MoveLineEnd();
            fixture.AutoComplete = (_, _, _) => ["init"];

            // When
            fixture.Execute(AutoComplete.Next);

            // Then
            fixture.Buffer.Content.ShouldBe("git init");
        }

        [Fact]
        public void Should_Replace_Previous_AutoComplete_Suggestion() {
            // Given
            var fixture = new Fixture();
            fixture.Buffer.Insert("git ");
            fixture.Buffer.MoveLineEnd();
            fixture.AutoComplete = (_, _, _) => ["init", "push"];

            // When
            fixture.Execute(AutoComplete.Next);
            fixture.Execute(AutoComplete.Next);

            // Then
            fixture.Buffer.Content.ShouldBe("git push");
        }

        [Fact]
        public void Should_Replace_Previous_AutoComplete_Suggestion_If_Buffer_Has_Suffix() {
            // Given
            var fixture = new Fixture();
            fixture.Buffer.Insert("git  -m 'Hello World'");
            fixture.Buffer.SetPosition(4);
            fixture.AutoComplete = (_, _, _) => ["init", "push"];

            // When
            fixture.Execute(AutoComplete.Next);
            fixture.Execute(AutoComplete.Next);

            // Then
            fixture.Buffer.Content.ShouldBe("git push -m 'Hello World'");
        }
    }

    public sealed class Previous {
        [Fact]
        public void Should_Insert_Last_Autocomplete_Suggestion() {
            // Given
            var fixture = new Fixture();
            fixture.Buffer.Insert("git ");
            fixture.Buffer.MoveLineEnd();
            fixture.AutoComplete = (_, _, _) => ["init", "push"];

            // When
            fixture.Execute(AutoComplete.Previous);

            // Then
            fixture.Buffer.Content.ShouldBe("git push");
        }

        [Fact]
        public void Should_Replace_Previous_AutoComplete_Suggestion() {
            // Given
            var fixture = new Fixture();
            fixture.Buffer.Insert("git ");
            fixture.Buffer.MoveLineEnd();
            fixture.AutoComplete = (_, _, _) => ["init", "push"];

            // When
            fixture.Execute(AutoComplete.Previous);
            fixture.Execute(AutoComplete.Previous);

            // Then
            fixture.Buffer.Content.ShouldBe("git init");
        }

        [Fact]
        public void Should_Replace_Previous_AutoComplete_Suggestion_If_Buffer_Has_Suffix() {
            // Given
            var fixture = new Fixture();
            fixture.Buffer.Insert("git  --quiet");
            fixture.Buffer.SetPosition(4);
            fixture.AutoComplete = (_, _, _) => ["init", "push"];

            // When
            fixture.Execute(AutoComplete.Previous);
            fixture.Execute(AutoComplete.Previous);

            // Then
            fixture.Buffer.Content.ShouldBe("git init --quiet");
        }
    }
}
