namespace RadLine;

public sealed class LineBufferTests {
    public sealed class TheMoveMethod {
        [Fact]
        public void Should_Move_To_Position() {
            // Given
            var buffer = new LineBuffer("FOO");

            // When
            buffer.SetPosition(1);

            // Then
            buffer.Position.ShouldBe(1);
        }

        [Fact]
        public void Should_Not_Move_If_Already_At_Position() {
            // Given
            var buffer = new LineBuffer("FOO");
            buffer.SetPosition(3);

            // When
            buffer.SetPosition(3);

            // Then
            buffer.Position.ShouldBe(3);
        }
    }

    public sealed class TheInsertCharacterMethod {
        [Fact]
        public void Should_Insert_Character() {
            // Given
            var buffer = new LineBuffer();

            // When
            buffer.Insert('A');

            // Then
            buffer.Content.ShouldBe("A");
        }

        [Fact]
        public void Should_Not_Move_Caret_Position() {
            // Given
            var buffer = new LineBuffer();

            // When
            buffer.Insert('A');

            // Then
            buffer.Position.ShouldBe(0);
        }
    }

    public sealed class TheMoveEndMethod {
        [Fact]
        public void Should_Move_To_End_Of_Buffer() {
            // Given
            var buffer = new LineBuffer();
            buffer.Insert("ABC");

            // When
            buffer.MoveLineEnd();

            // Then
            buffer.Position.ShouldBe(3);
        }
    }

    public sealed class TheMoveRightMethod {
        [Fact]
        public void Should_Move_Right_If_Caret_Is_Not_At_End_Of_Line() {
            // Given
            var buffer = new LineBuffer();
            buffer.Insert('A');

            // When
            buffer.MoveRight();

            // Then
            buffer.Position.ShouldBe(1);
        }

        [Fact]
        public void Should_Move_Past_Grapheme_Cluster() {
            // Given
            var buffer = new LineBuffer();
            buffer.Insert("😄A");

            // When
            buffer.MoveRight();

            // Then
            buffer.Position.ShouldBe(2);
        }

        [Fact]
        public void Should_Not_Move_Right_If_Position_Is_At_End_Of_Line() {
            // Given
            var buffer = new LineBuffer();

            // When
            buffer.MoveRight();

            // Then
            buffer.Position.ShouldBe(0);
        }
    }

    public sealed class TheMoveHomeMethod {
        [Fact]
        public void Should_Move_To_Start_Of_Buffer() {
            // Given
            var buffer = new LineBuffer();
            buffer.Insert("A");
            buffer.MoveLineEnd();

            // When
            buffer.MoveLineStart();

            // Then
            buffer.Position.ShouldBe(0);
        }
    }

    public sealed class TheMoveToPreviousWordMethod {
        [Fact]
        public void Should_Move_To_The_Previous_Word_If_Position_Is_At_End_Of_Line() {
            // Given
            var buffer = new LineBuffer("Foo Bar Baz");

            // When
            buffer.MoveToPreviousWord();

            // Then
            buffer.Position.ShouldBe(8);
        }

        [Fact]
        public void Should_Move_To_Left_Word_Boundary_If_Position_Is_Inside_Word() {
            // Given
            var buffer = new LineBuffer("Foo Bar Baz");
            buffer.MoveLeft();

            // When
            buffer.MoveToPreviousWord();

            // Then
            buffer.Position.ShouldBe(8);
        }

        [Fact]
        public void Should_Move_To_Previous_If_Position_Is_At_Beginning_Of_Word() {
            // Given
            var buffer = new LineBuffer("Foo Bar Baz");
            buffer.SetPosition(8);

            // When
            buffer.MoveToPreviousWord();

            // Then
            buffer.Position.ShouldBe(4);
        }
    }

    public sealed class TheMoveToNextWordMethod {
        [Fact]
        public void Should_Move_To_The_Beginning_Of_The_Next_Word() {
            // Given
            var buffer = new LineBuffer("Foo Bar");
            buffer.MoveLineStart();

            // When
            buffer.MoveToNextWord();

            // Then
            buffer.Position.ShouldBe(4);
        }

        [Fact]
        public void Should_Move_To_The_End_Of_The_Last_Word_If_There_Is_No_Next_Word() {
            // Given
            var buffer = new LineBuffer("Foo");
            buffer.MoveLineStart();

            // When
            buffer.MoveToNextWord();

            // Then
            buffer.Position.ShouldBe(3);
        }

        [Fact]
        public void Should_Move_To_Next_Word_If_Position_Is_Between_Two_Words() {
            // Given
            var buffer = new LineBuffer("Foo Bar Baz");
            buffer.SetPosition(3);

            // When
            buffer.MoveToNextWord();

            // Then
            buffer.Position.ShouldBe(4);
        }
    }

    public sealed class TheMoveLeftMethod {
        [Fact]
        public void Should_Move_Left_If_Position_Is_Greater_Than_Zero() {
            // Given
            var buffer = new LineBuffer();
            buffer.Insert("ABC");
            buffer.MoveLineEnd();

            // When
            buffer.MoveLeft();

            // Then
            buffer.Position.ShouldBe(2);
        }

        [Fact]
        public void Should_Move_Past_Grapheme_Cluster() {
            // Given
            var buffer = new LineBuffer();
            buffer.Insert("😄");
            buffer.MoveLineEnd();

            // When
            buffer.MoveLeft();

            // Then
            buffer.Position.ShouldBe(0);
        }

        [Fact]
        public void Should_Not_Move_Left_If_Position_Is_At_Beginning_Of_Line() {
            // Given
            var buffer = new LineBuffer();

            // When
            buffer.MoveLeft();

            // Then
            buffer.Position.ShouldBe(0);
        }
    }

    public sealed class TheClearMethod {
        [Fact]
        public void Should_Delete_Previous_Character() {
            // Given
            var buffer = new LineBuffer();
            buffer.Insert("AB");

            // When
            buffer.Clear(1, 1);

            // Then
            buffer.Content.ShouldBe("A");
        }

        [Fact]
        public void Should_Not_Delete_If_Index_Is_Past_Buffer_Length() {
            // Given
            var buffer = new LineBuffer();
            buffer.Insert("AB");

            // When
            buffer.Clear(3, 1);

            // Then
            buffer.Content.ShouldBe("AB");
        }

        [Fact]
        public void Should_Not_Delete_Past_Buffer() {
            // Given
            var buffer = new LineBuffer();
            buffer.Insert("AB");

            // When
            buffer.Clear(1, 3);

            // Then
            buffer.Content.ShouldBe("A");
        }
    }
}
