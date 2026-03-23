using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;


namespace Digrafos.Views;

public class NodeSpawner
{
    private readonly Canvas _canvas;
    private readonly GridBackgroundControl _grid;
    
    private readonly List<NodeView> _activeNodes = new();
    private readonly Dictionary<NodeView, (Vector2 GridPos, NodeView Parent)> _availableSlots = new();
    
    private bool _isBuildModeActive = false;

    public NodeView? RootNode { get; private set; }
    public IReadOnlyList<NodeView> AllNodes => _activeNodes;

    public NodeSpawner(Canvas canvas, GridBackgroundControl grid)
    {
        _canvas = canvas;
        _grid = grid;
    }

    public void ListenForPlacement(string content = "")
    {
        ToggleBuildMode(true);
    }

    public void ToggleBuildMode(bool isActive)
    {
        _isBuildModeActive = isActive;

        if (_isBuildModeActive)
        {
            if (_activeNodes.Count == 0)
            {
                SpawnRootNode(new Vector2(10, 2), "A");
            }
            
            ShowAvailableSlots();
            _canvas.PointerPressed += OnCanvasClicked;
        }
        else
        {
            HideAvailableSlots();
            _canvas.PointerPressed -= OnCanvasClicked;
        }
    }

    private void SpawnRootNode(Vector2 gridPos, string content)
    {
        var pixelPos = _grid.GridToCanvas(gridPos);
        var node = new NodeView(content, false);
        
        Canvas.SetLeft(node, pixelPos.X - node.Width / 2);
        Canvas.SetTop(node, pixelPos.Y - node.Height / 2);
        
        _canvas.Children.Add(node);
        _activeNodes.Add(node);
        RootNode = node;
        _grid.AddPoint(gridPos, Avalonia.Media.Colors.Transparent);
    }

    private void ShowAvailableSlots()
    {
        HideAvailableSlots(); 

        foreach (var activeNode in _activeNodes)
        {
            var left = Canvas.GetLeft(activeNode);
            var top = Canvas.GetTop(activeNode);
            var pixelPos = new Avalonia.Point(left + activeNode.Width / 2, top + activeNode.Height / 2);
            var gridPos = PixelToGrid(pixelPos);

            var leftChildPos = new Vector2(gridPos.X - 1.5f, gridPos.Y + 2);
            var rightChildPos = new Vector2(gridPos.X + 1.5f, gridPos.Y + 2);

            CreateSlotIfNotOccupied(leftChildPos, activeNode);
            CreateSlotIfNotOccupied(rightChildPos, activeNode);
        }
    }

    private void CreateSlotIfNotOccupied(Vector2 gridPos, NodeView parentNode)
    {
        foreach (var activeNode in _activeNodes)
        {
            var left = Canvas.GetLeft(activeNode);
            var top = Canvas.GetTop(activeNode);
            var pixelPos = new Avalonia.Point(left + activeNode.Width / 2, top + activeNode.Height / 2);
            var nodeGridPos = PixelToGrid(pixelPos);

            if (Math.Abs(nodeGridPos.X - gridPos.X) < 0.1f && Math.Abs(nodeGridPos.Y - gridPos.Y) < 0.1f)
            {
                return;
            }
        }

        foreach (var slot in _availableSlots.Values)
        {
            if (Math.Abs(slot.GridPos.X - gridPos.X) < 0.1f && Math.Abs(slot.GridPos.Y - gridPos.Y) < 0.1f)
            {
                return;
            }
        }

        var pixelPosFinal = _grid.GridToCanvas(gridPos);
        var slotNode = new NodeView("", true); 
        
        Canvas.SetLeft(slotNode, pixelPosFinal.X - slotNode.Width / 2);
        Canvas.SetTop(slotNode, pixelPosFinal.Y - slotNode.Height / 2);
        
        _canvas.Children.Add(slotNode);
        _availableSlots.Add(slotNode, (gridPos, parentNode));
    }

    private void HideAvailableSlots()
    {
        foreach (var slot in _availableSlots.Keys)
        {
            _canvas.Children.Remove(slot);
        }
        _availableSlots.Clear();
    }

    private void OnCanvasClicked(object? sender, PointerPressedEventArgs e)
    {
        if (!_isBuildModeActive || !e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed) return;

        var clickPos = e.GetPosition(_canvas);
        NodeView? clickedSlot = null;

        foreach (var slot in _availableSlots.Keys)
        {
            var left = Canvas.GetLeft(slot);
            var top = Canvas.GetTop(slot);
            var rect = new Rect(left, top, slot.Width, slot.Height);

            if (rect.Contains(clickPos))
            {
                clickedSlot = slot;
                break;
            }
        }

        if (clickedSlot != null)
        {
            var slotData = _availableSlots[clickedSlot];
            
            string newName = ((char)('A' + _activeNodes.Count)).ToString();
            
            clickedSlot.ConvertToRealNode(newName);
            clickedSlot.ParentNode = slotData.Parent;
            
            /*
            Al vincular el nuevo nodo como hijo de su padre en la jerarquia de datos 
            permitimos que los algoritmos de busqueda puedan navegar el grafo de forma estructurada.
            */
            slotData.Parent.Children.Add(clickedSlot);
            
            ConnectionBuilder.DrawDirectedConnection(_canvas, slotData.Parent, clickedSlot);
            
            _activeNodes.Add(clickedSlot);
            _availableSlots.Remove(clickedSlot);
            _grid.AddPoint(slotData.GridPos, Avalonia.Media.Colors.Transparent);

            ShowAvailableSlots(); 
            e.Handled = true;
        }
    }

    private Vector2 PixelToGrid(Avalonia.Point pixel) =>
        new((float)(pixel.X / _grid.GridCellSize),
            (float)(pixel.Y / _grid.GridCellSize));
}