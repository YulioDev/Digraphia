using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Digraphia.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Digraphia.Views;

public class NodeSpawner
{
    private readonly Canvas _canvas;
    private readonly GridBackgroundControl _grid;
    private Tree _tree = new();
    private readonly Dictionary<Node, NodeView> _nodeToView = new();

    private Node? _lastSelectedNode;
    private int _nodeCounter = 0;

    public Node? RootNode => _tree.Root;
    public IReadOnlyList<Node> AllNodes => _tree.AllNodes;

    public NodeSpawner(Canvas canvas, GridBackgroundControl grid)
    {
        _canvas = canvas;
        _grid = grid;
        _tree = new Tree();

        _canvas.PointerPressed += OnCanvasClicked;
    }

    // Genera identificadores visuales secuenciales (A, B, C... Z, AA, AB...)
    private string GenerateNodeName()
    {
        int value = _nodeCounter++;
        string name = string.Empty;
        do
        {
            name = (char)('A' + (value % 26)) + name;
            value = (value / 26) - 1;
        } while (value >= 0);
        return name;
    }

    // Optimización GC: Uso de loop for nativo y cálculo de distancia al cuadrado 
    // para evitar asignaciones de memoria y el uso pesado de Math.Sqrt
    private Node? FindClosestNode(Vector2 position)
    {
        var nodes = _tree.AllNodes;
        if (nodes.Count == 0) return null;

        Node? closest = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < nodes.Count; i++)
        {
            float dist = nodes[i].GridPosition.DistanceSquaredTo(position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = nodes[i];
            }
        }

        return closest;
    }

    private void OnCanvasClicked(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed) return;

        var mode = Globals.Instance.CurrentMode;
        var clickPos = e.GetPosition(_canvas);
        var clickedNode = GetNodeAtPosition(clickPos);

        switch (mode)
        {
            case EditorMode.BuildNode:
                HandleBuildMode(clickPos, clickedNode);
                break;
            case EditorMode.RemoveNode:
                if (clickedNode != null) HandleRemoveMode(clickedNode);
                break;
            case EditorMode.GoalMode:
                if (clickedNode != null) HandleGoalMode(clickedNode);
                break;
        }
    }

    private void HandleBuildMode(Point clickPos, Node? clickedNode)
    {
        if (clickedNode == null)
        {
            var snappedPos = SnapToGrid(clickPos);
            var gridVec = new Vector2((float)snappedPos.X, (float)snappedPos.Y);

            var closestNode = FindClosestNode(gridVec);
            var newNode = new Node(GenerateNodeName(), gridVec);

            _tree.AddNode(newNode);

            // Conexión automática al nodo más cercano al momento de crearlo
            if (closestNode != null)
            {
                closestNode.Children.Add(newNode);
                newNode.Parent = closestNode;
                ConsoleService.Output($"Autoconectado: {closestNode.Id} -> {newNode.Id}");
            }

            var nodeView = new NodeView { DataContext = newNode };
            Canvas.SetLeft(nodeView, snappedPos.X - 25);
            Canvas.SetTop(nodeView, snappedPos.Y - 25);

            _nodeToView[newNode] = nodeView;
            _canvas.Children.Add(nodeView);

            RefreshConnections();
            _lastSelectedNode = null;
        }
        else
        {
            // Permite al usuario forzar conexiones manuales adicionales haciendo clic en dos nodos
            if (_lastSelectedNode == null)
            {
                _lastSelectedNode = clickedNode;
                ConsoleService.Output($"Nodo {clickedNode.Id} seleccionado. Clic en otro para conectar.");
            }
            else if (_lastSelectedNode != clickedNode)
            {
                if (!_lastSelectedNode.Children.Contains(clickedNode))
                {
                    _lastSelectedNode.Children.Add(clickedNode);
                    clickedNode.Parent = _lastSelectedNode;
                    RefreshConnections();
                    ConsoleService.Output($"Conectado: {_lastSelectedNode.Id} -> {clickedNode.Id}");
                }
                _lastSelectedNode = null;
            }
        }
    }

    private void HandleRemoveMode(Node clickedNode)
    {
        RemoveNode(clickedNode);
        RefreshConnections();
        ConsoleService.Output($"Nodo {clickedNode.Id} eliminado.");
    }

    private void HandleGoalMode(Node clickedNode)
    {
        var nodes = _tree.AllNodes;
        for (int i = 0; i < nodes.Count; i++) nodes[i].IsGoal = false;

        clickedNode.IsGoal = true;
        ConsoleService.Output($"Nodo {clickedNode.Id} marcado como Meta.");
    }

    private Vector2 SnapToGrid(Point p)
    {
        float size = _grid.GridCellSize;
        float x = (float)(Math.Round(p.X / size) * size);
        float y = (float)(Math.Round(p.Y / size) * size);
        return new Vector2(x, y);
    }

    private Node? GetNodeAtPosition(Point clickPos)
    {
        var view = _nodeToView.Values.FirstOrDefault(v =>
        {
            var left = Canvas.GetLeft(v);
            var top = Canvas.GetTop(v);
            var rect = new Rect(left, top, v.Width, v.Height);
            return rect.Contains(clickPos);
        });
        return view?.DataContext as Node;
    }

    public NodeView? GetViewForNode(Node node)
    {
        return _nodeToView.TryGetValue(node, out var view) ? view : null;
    }

    private void RemoveNode(Node node)
    {
        _tree.RemoveNode(node);
        if (_nodeToView.TryGetValue(node, out var view))
        {
            _canvas.Children.Remove(view);
            _nodeToView.Remove(node);
        }
    }

    private void RefreshConnections()
    {
        var toRemove = new List<Control>();
        toRemove.AddRange(_canvas.Children.OfType<Line>());
        toRemove.AddRange(_canvas.Children.OfType<Polygon>());

        int removeCount = toRemove.Count;
        for (int i = 0; i < removeCount; i++) _canvas.Children.Remove(toRemove[i]);

        var nodes = _tree.AllNodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            int childCount = node.Children.Count;

            for (int j = 0; j < childCount; j++)
            {
                var child = node.Children[j];
                if (_nodeToView.TryGetValue(node, out var parentView) &&
                    _nodeToView.TryGetValue(child, out var childView))
                {
                    ConnectionBuilder.DrawDirectedConnection(_canvas, parentView, childView);
                }
            }
        }
    }
}