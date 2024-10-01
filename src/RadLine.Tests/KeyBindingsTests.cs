namespace RadLine;

public sealed class KeyBindingsTests {
    [Fact]
    public void Should_Have_No_Bindings_When_Created() {
        // Given
        var bindings = new KeyBindings();

        // When
        var result = bindings.Count;

        // Then
        result.ShouldBe(0);
    }

    public sealed class TheAddMethod {
        [Fact]
        public void Should_Add_Command() {
            // Given
            var bindings = new KeyBindings();
            bindings.Add<MoveLineEndCommand>(ConsoleKey.Home);

            // When
            var result = bindings.Count;

            // Then
            result.ShouldBe(1);
        }
    }

    public sealed class TheRemoveMethod {
        [Fact]
        public void Should_Remove_Command() {
            // Given
            var bindings = new KeyBindings();
            bindings.Add<MoveLineEndCommand>(ConsoleKey.Home);
            bindings.Remove(ConsoleKey.Home);

            // When
            var command = bindings.GetCommand(ConsoleKey.Home);

            // Then
            bindings.Count.ShouldBe(0);
            command.ShouldBeNull();
        }
    }

    public sealed class TheGetCommandMethod {
        [Fact]
        public void Should_Get_Command_For_KeyBinding_Without_Modifier() {
            // Given
            var bindings = new KeyBindings();

            bindings.Add<MoveLineEndCommand>(ConsoleKey.End);
            bindings.Add<MoveLineStartCommand>(ConsoleKey.Home);

            // When
            var command = bindings.GetCommand(ConsoleKey.Home, null);

            // Then
            command.ShouldBeOfType<MoveLineStartCommand>();
        }

        [Fact]
        public void Should_Get_Command_For_KeyBinding_With_Modifier() {
            // Given
            var bindings = new KeyBindings();

            bindings.Add<MoveLineEndCommand>(ConsoleKey.Home);
            bindings.Add<MoveLineStartCommand>(ConsoleKey.Home, ConsoleModifiers.Shift);

            // When
            var command = bindings.GetCommand(ConsoleKey.Home, ConsoleModifiers.Shift);

            // Then
            command.ShouldBeOfType<MoveLineStartCommand>();
        }

        [Fact]
        public void Should_Not_Get_Command_For_KeyBinding_With_Modifier_When_No_Modifier_Was_Provided() {
            // Given
            var bindings = new KeyBindings();

            bindings.Add<MoveLineEndCommand>(ConsoleKey.End);
            bindings.Add<MoveLineStartCommand>(ConsoleKey.Home, ConsoleModifiers.Shift);

            // When
            var command = bindings.GetCommand(ConsoleKey.Home);

            // Then
            command.ShouldBeNull();
        }
    }
}
