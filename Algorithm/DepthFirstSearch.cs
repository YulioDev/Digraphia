using System;
using System.Collections.Generic;
using System.Buffers;
using Avalonia.Threading;
using Digraphia;
using Digraphia.Services;

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
    }

    public void Reset()
    {
        _stack.Clear();
        _visited.Clear();
        _current = null;
        _finished = false;
    }

    public Node? GetCurrentNode() => _current;

    public bool Step()
    {
        if (_finished) { return false; }

        if (_stack.Count == 0)
        {
            _finished = true;
            if (_current != null)
            {
                var last = _current;
                Dispatcher.UIThread.Post(() => last.State = NodeState.Completed);
            }
            return false;
        }

        var nextNode = _stack.Pop();

        if (_current != null && _current != nextNode)
        {
            var prev = _current;
            Dispatcher.UIThread.Post(() => prev.State = NodeState.Completed);
        }

        _current = nextNode;

        if (_visited.Contains(_current))
        {
            ConsoleService.Output($"Omitiendo nodo ya visitado {_current.Id}");
            return Step();
        }

        _visited.Add(_current);
        Dispatcher.UIThread.Post(() => _current.State = NodeState.Highlight);

        int childCount = _current.Children.Count;
        if (childCount > 0)
        {
            // Conecta con System.Buffers. Rol: Evita saturar el GC pidiendo memoria prestada en lugar de crear listas nuevas por cada salto de nodo.
            var temp = ArrayPool<Node>.Shared.Rent(childCount);
            for (int i = 0; i < childCount; i++) { temp[i] = _current.Children[i]; }

            // Conecta con GridPosition. Rol: Ordena los hijos basándose en su coordenada X (Derecha a Izquierda). Al apilarse, el de menor X (Izquierda) quedará en la cima y será el próximo en ejecutarse.
            Array.Sort(temp, 0, childCount, Comparer<Node>.Create((a, b) => b.GridPosition.X.CompareTo(a.GridPosition.X)));

            for (int i = 0; i < childCount; i++)
            {
                if (!_visited.Contains(temp[i])) { _stack.Push(temp[i]); }
            }

            ArrayPool<Node>.Shared.Return(temp, clearArray: true);
        }

        return true;
    }
}