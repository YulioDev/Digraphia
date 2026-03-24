using System.Collections.Generic;
using Digraphia.Views;
using Digraphia;
namespace Digraphia.Algorithms;

public class DepthFirstSearch : IGraphAlgorithm<Node>
{
    private readonly Stack<Node> _stack = new();
    private readonly HashSet<Node> _visited = new();
    private Node? _current;
    private bool _finished;

    public bool IsFinished => _finished;

    public void Initialize(Node start)
    {
        Reset();
        _stack.Push(start);
        _current = null;
    }

    public bool Step()
    {
        if (_finished) return false;

        if (_stack.Count == 0)
        {
            _finished = true;
            return false;
        }

        _current = _stack.Pop();

        if (_visited.Contains(_current))
        {
            // Si ya estaba visitado, es porque estamos retrocediendo
            // Marcar como completado (opcional)
            _current.State = NodeState.Completed;
            return Step(); // continuar con el siguiente
        }

        _visited.Add(_current);
        _current.State = NodeState.Highlight; // visitado

        // Agregar hijos a la pila en orden inverso
        for (int i = _current.Children.Count - 1; i >= 0; i--)
        {
            var child = _current.Children[i];
            if (!_visited.Contains(child))
                _stack.Push(child);
        }

        return _stack.Count > 0;
    }

    public Node? GetCurrentNode() => _current;

    public void Reset()
    {
        _stack.Clear();
        _visited.Clear();
        _current = null;
        _finished = false;
    }
}