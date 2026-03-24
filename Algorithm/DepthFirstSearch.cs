using System.Collections.Generic;
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

    // Método expuesto para que el Runner sepa dónde está el foco visual.
    public Node? GetCurrentNode() => _current;

    public bool Step()
    {
        if (_finished) return false;

        // Si la pila está vacía, marcamos el último nodo y cerramos la ejecución.
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

        // Limpieza visual: El nodo que acabamos de dejar pasa a estar completado.
        if (_current != null && _current != nextNode)
        {
            var prev = _current;
            Dispatcher.UIThread.Post(() => prev.State = NodeState.Completed);
        }

        _current = nextNode;

        // Si ya lo visitamos, omitimos recursivamente sin romper el ciclo del Runner.
        if (_visited.Contains(_current))
        {
            ConsoleService.Output($"Omitiendo nodo ya visitado {_current.Id}");
            return Step();
        }

        _visited.Add(_current);
        Dispatcher.UIThread.Post(() => _current.State = NodeState.Highlight);

        // Agregamos en reversa para mantener el orden de visita visual natural de izquierda a derecha.
        for (int i = _current.Children.Count - 1; i >= 0; i--)
        {
            var child = _current.Children[i];
            if (!_visited.Contains(child)) _stack.Push(child);
        }

        // Siempre retornamos true si procesamos un nodo, indicando al Runner que aplique el Delay.
        return true;
    }
}