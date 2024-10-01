namespace RadLine;

public interface ITextEditorHistory {
    int Count { get; }

    void Add(string text);
}
