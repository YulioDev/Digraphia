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
    private readonly List<NodeView> _activeNodes = new();
    private bool _isBuildModeActive;
    private bool _isRemoveModeActive;

    public NodeView? RootNode { get; private set; }
    public IReadOnlyList<NodeView> AllNodes => _activeNodes;

    public NodeSpawner(Canvas canvas, GridBackgroundControl grid)
    {
        _canvas = canvas;
        _grid = grid;
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
            // Obtener celda de grilla más cercana
            var gridPos = PixelToGrid(clickPos);
            // Redondear a coordenadas enteras para alineación
            gridPos = new Vector2((float)Math.Round(gridPos.X), (float)Math.Round(gridPos.Y));

            // Verificar si ya existe un nodo en esa celda
            if (IsNodeAtPosition(gridPos))
                return;

            // Crear nuevo nodo
            var newNode = CreateNode(gridPos);
            if (newNode != null)
            {
                // Conectar al nodo más cercano (si existe)
                var nearest = FindNearestNode(newNode);
                if (nearest != null)
                {
                    nearest.Children.Add(newNode);
                    newNode.ParentNode = nearest;
                }
                else if (_activeNodes.Count == 0)
                {
                    // Es el primer nodo, será la raíz
                    RootNode = newNode;
                    // Centrar la cámara en la raíz (lo maneja DrawingAreaView)
                }

                _activeNodes.Add(newNode);
                _grid.AddPoint(gridPos, Colors.Transparent); // punto de ocupación
                RefreshConnections();
            }
        }
        else if (_isRemoveModeActive)
        {
            // Buscar nodo bajo el cursor
            var node = FindNodeAtPosition(clickPos);
            if (node != null)
            {
                RemoveNode(node);
                RefreshConnections();
            }
        }
    }

    private NodeView? CreateNode(Vector2 gridPos)
    {
        var pixelPos = _grid.GridToCanvas(gridPos);
        string nodeName = ((char)('A' + _activeNodes.Count)).ToString();
        var node = new NodeView(nodeName, false);

        Canvas.SetLeft(node, pixelPos.X - node.Width / 2);
        Canvas.SetTop(node, pixelPos.Y - node.Height / 2);

        _canvas.Children.Add(node);
        return node;
    }

    private bool IsNodeAtPosition(Vector2 gridPos)
    {
        return _activeNodes.Any(node =>
        {
            var left = Canvas.GetLeft(node);
            var top = Canvas.GetTop(node);
            var center = new Avalonia.Point(left + node.Width / 2, top + node.Height / 2);
            var nodeGrid = PixelToGrid(center);
            return Math.Abs(nodeGrid.X - gridPos.X) < 0.1f && Math.Abs(nodeGrid.Y - gridPos.Y) < 0.1f;
        });
    }

    private NodeView? FindNodeAtPosition(Avalonia.Point clickPos)
    {
        return _activeNodes.FirstOrDefault(node =>
        {
            var left = Canvas.GetLeft(node);
            var top = Canvas.GetTop(node);
            var rect = new Rect(left, top, node.Width, node.Height);
            return rect.Contains(clickPos);
        });
    }

    private NodeView? FindNearestNode(NodeView newNode)
    {
        if (_activeNodes.Count == 0)
            return null;

        var newNodePos = GetNodeGridPosition(newNode);
        NodeView? nearest = null;
        double minDist = double.MaxValue;

        foreach (var node in _activeNodes)
        {
            var nodePos = GetNodeGridPosition(node);
            double dx = newNodePos.X - nodePos.X;
            double dy = newNodePos.Y - nodePos.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = node;
            }
        }
        return nearest;
    }

    private Vector2 GetNodeGridPosition(NodeView node)
    {
        var left = Canvas.GetLeft(node);
        var top = Canvas.GetTop(node);
        var center = new Avalonia.Point(left + node.Width / 2, top + node.Height / 2);
        return PixelToGrid(center);
    }

    private void RemoveNode(NodeView node)
    {
        // Eliminar referencias en el padre
        if (node.ParentNode != null)
            node.ParentNode.Children.Remove(node);

        // Eliminar referencias en los hijos (ellos pierden padre)
        foreach (var child in node.Children.ToList())
        {
            child.ParentNode = null;
            // Opcional: eliminar también los hijos (decidir si se eliminan en cascada)
            // Aquí simplemente se desconectan
        }
        node.Children.Clear();

        // Remover del canvas y de la lista activa
        _canvas.Children.Remove(node);
        _activeNodes.Remove(node);

        // Si era la raíz, reasignar raíz si queda algún nodo
        if (node == RootNode)
            RootNode = _activeNodes.FirstOrDefault();

        // Limpiar punto de ocupación en la grilla (opcional)
        var gridPos = GetNodeGridPosition(node);
        // _grid.RemovePoint(gridPos);  // si se implementa RemovePoint en GridBackgroundControl
    }

    private void RefreshConnections()
    {
        // Eliminar todas las líneas y flechas existentes en el canvas
        var toRemove = new List<Control>();
        toRemove.AddRange(_canvas.Children.OfType<Line>());
        toRemove.AddRange(_canvas.Children.OfType<Polygon>());

        foreach (var element in toRemove)
            _canvas.Children.Remove(element);

        // Redibujar todas las conexiones
        foreach (var node in _activeNodes)
        {
            foreach (var child in node.Children)
            {
                ConnectionBuilder.DrawDirectedConnection(_canvas, node, child);
            }
        }
    }

    private Vector2 PixelToGrid(Avalonia.Point pixel) =>
        new((float)(pixel.X / _grid.GridCellSize),
            (float)(pixel.Y / _grid.GridCellSize));
}