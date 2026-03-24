namespace Digraphia.Algorithms;

public interface IGraphAlgorithm<T>
{
    void Initialize(T start);
    bool Step();
    bool IsFinished { get; }
    T? GetCurrentNode();
    void Reset();
}