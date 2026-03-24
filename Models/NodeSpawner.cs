using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

namespace Digraphia.Views;

public class NodeSpawner
{
    private readonly Canvas _canvas;
    private readonly GridBackgroundControl _grid;
    private Tree _tree = new();
    private readonly Dictionary<Node, NodeView> _nodeToView = new();

    private bool _isBuildModeActive;
    private bool _isRemoveModeActive;

    public Node? RootNode => _tree.Root;
    public IReadOnlyList<Node> AllNodes => _tree.AllNodes;

    public NodeSpawner(Canvas canvas, GridBackgroundControl grid)
    {
        _canvas = canvas;
        _grid = grid;
        _tree = new Tree();  // Inicialización explícita
    }

    public void EnableBuildMode()
    {
        _isBuildModeActive = true;
        _isRemoveModeActive = false;
        _canvas.PointerPressed += OnCanvasClicked;
    }

    public void EnableRemoveMode()
    {
        _isRemoveModeActive = true;
        _isBuildModeActive = false;
        _canvas.PointerPressed += OnCanvasClicked;
    }

    public void DisableAllModes()
    {
        _isBuildModeActive = false;
        _isRemoveModeActive = false;
        _canvas.PointerPressed -= OnCanvasClicked;
    }

    private void OnCanvasClicked(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed)
            return;

        var clickPos = e.GetPosition(_canvas);

        if (_isBuildModeActive)
        {
            var gridPos = PixelToGrid(clickPos);
            gridPos = new Vector2((float)Math.Round(gridPos.X), (float)Math.Round(gridPos.Y));

            if (IsNodeAtPosition(gridPos))
                return;

            var newNode = CreateNode(gridPos);
            if (newNode != null)
            {
                var nearest = _tree.FindNearestNode(gridPos);
                if (nearest != null && nearest != newNode)
                {
                    nearest.Children.Add(newNode);
                    newNode.Parent = nearest;
                }
                else if (_tree.AllNodes.Count == 0)
                {
                    // primer nodo, será raíz
                }

                _tree.AddNode(newNode);
                _grid.AddPoint(gridPos, Colors.Transparent);
                RefreshConnections();
            }
        }
        else if (_isRemoveModeActive)
        {
            var node = FindNodeAtPosition(clickPos);
            if (node != null)
            {
                RemoveNode(node);
                RefreshConnections();
            }
        }
    }

    private Node? CreateNode(Vector2 gridPos)
    {
        var pixelPos = _grid.GridToCanvas(gridPos);
        string nodeId = ((char)('A' + _tree.AllNodes.Count)).ToString();
        var node = new Node(nodeId, gridPos);

        // Crear vista y enlazar
        var view = new NodeView
        {
            DataContext = node,
            Width = 40,
            Height = 40
        };
        Canvas.SetLeft(view, pixelPos.X - view.Width / 2);
        Canvas.SetTop(view, pixelPos.Y - view.Height / 2);
        _canvas.Children.Add(view);

        _nodeToView[node] = view;
        return node;
    }

    private bool IsNodeAtPosition(Vector2 gridPos)
    {
        // Asegurar que ningún nodo tenga posición nula
        return _tree.AllNodes.Any(n => n.GridPosition.DistanceTo(gridPos) < 0.1f);
    }

    private Node? FindNodeAtPosition(Avalonia.Point clickPos)
    {
        // Buscar primero por colisión en la vista
        var view = _canvas.Children.OfType<NodeView>().FirstOrDefault(v =>
        {
            var left = Canvas.GetLeft(v);
            var top = Canvas.GetTop(v);
            var rect = new Rect(left, top, v.Width, v.Height);
            return rect.Contains(clickPos);
        });
        if (view != null)
            return view.DataContext as Node;
        return null;
    }
    // Agregar dentro de la clase NodeSpawner:
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
        foreach (var element in toRemove)
            _canvas.Children.Remove(element);

        foreach (var node in _tree.AllNodes)
        {
            foreach (var child in node.Children)
            {
                if (_nodeToView.TryGetValue(node, out var parentView) &&
                    _nodeToView.TryGetValue(child, out var childView))
                {
                    ConnectionBuilder.DrawDirectedConnection(_canvas, parentView, childView);
                }
            }
        }
    }

    private Vector2 PixelToGrid(Avalonia.Point pixel) =>
        new((float)(pixel.X / _grid.GridCellSize),
            (float)(pixel.Y / _grid.GridCellSize));
}