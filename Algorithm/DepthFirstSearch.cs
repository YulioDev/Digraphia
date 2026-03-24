using System.Collections.Generic;
using Digraphia.Views;

namespace Digraphia.Algorithms;

public class DepthFirstSearch : IGraphAlgorithm
{
    private readonly Stack<NodeView> _stack = new();
    private readonly HashSet<NodeView> _visited = new();
    private NodeView? _current;
    private bool _finished;

    public bool IsFinished => _finished;

    public void Initialize(NodeView start)
    {
        Reset();
        _stack.Push(start);
        _current = null;
    }

    public bool Step()
    {
        if (_finished)
            return false;

        if (_stack.Count == 0)
        {
            _finished = true;
            return false;
        }

        _current = _stack.Pop();

        if (_visited.Contains(_current))
        {
            // Si ya estaba visitado, continuar con el siguiente paso
            return Step();
        }

        _visited.Add(_current);

        // Empujar hijos en orden inverso para mantener el orden de exploración
        for (int i = _current.Children.Count - 1; i >= 0; i--)
        {
            var child = _current.Children[i];
            if (!_visited.Contains(child))
                _stack.Push(child);
        }

        return _stack.Count > 0;
    }

    public NodeView? GetCurrentNode() => _current;

    public void Reset()
    {
        _stack.Clear();
        _visited.Clear();
        _current = null;
        _finished = false;
    }
}