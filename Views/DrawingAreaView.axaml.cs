// DrawingAreaView.axaml.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Digraphia.Algorithms;
using Digraphia.EventManager;
using Digraphia.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Digraphia.Views;

public partial class DrawingAreaView : UserControl
{
    private bool _isPanning;
    private Point _lastMousePosition;
    private Canvas? _canvasDrawingArea;
    private GridBackgroundControl? _gridBackground;
    private TranslateTransform _transform = new();
    private NodeSpawner? _spawner;
    private GraphRunner? _currentRunner;
    private CancellationTokenSource? _currentCts;
    private Polyline? _pathPolyline;

    public DrawingAreaView()
    {
        InitializeComponent();
        _canvasDrawingArea = this.FindControl<Canvas>("canvasDrawingArea");
        _gridBackground = this.FindControl<GridBackgroundControl>("gridBackground");

        if (_canvasDrawingArea != null && _gridBackground != null)
        {
            _canvasDrawingArea.RenderTransform = _transform;
            _spawner = new NodeSpawner(_canvasDrawingArea, _gridBackground);
        }

        PointerPressed += Canvas_PointerPressed;
        PointerMoved += CanvasPointerMoved;
        PointerReleased += CanvasPointerReleased;
        PointerCaptureLost += (_, _) => _isPanning = false;
        LayoutUpdated += OnFirstLayout;

        MainEventManager.SearchStarted += OnSearchStarted;
        MainEventManager.SearchStopped += OnSearchStopped;
    }

    private async void OnSearchStarted(float speed)
    {
        if (_spawner == null) return;

        ClearPath();

        if (_currentRunner != null)
            _currentRunner.GoalReached -= OnGoalReached;

        OnSearchStopped();

        var root = _spawner.RootNode;
        if (root == null)
        {
            ConsoleService.Output("No hay nodos en el grafo para iniciar búsqueda.");
            return;
        }

        var nodes = _spawner.AllNodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (node.IsGoal) node.State = NodeState.Goal;
            else node.State = NodeState.Normal;
        }

        var algorithm = new DepthFirstSearch();
        _currentRunner = new GraphRunner(algorithm);
        _currentCts = new CancellationTokenSource();

        _currentRunner.GoalReached += OnGoalReached;

        float safeSpeed = speed <= 0.05f ? 0.05f : speed;

        try
        {
            await _currentRunner.RunAsync(root, safeSpeed, _currentCts.Token);
        }
        catch (Exception ex)
        {
            ConsoleService.Output($"Error inesperado: {ex.Message}");
        }
        finally
        {
            if (_currentRunner != null)
                _currentRunner.GoalReached -= OnGoalReached;
            _currentRunner = null;
            _currentCts?.Dispose();
            _currentCts = null;
        }
    }

    private void OnSearchStopped()
    {
        _currentCts?.Cancel();
    }

    private void OnFirstLayout(object? sender, EventArgs e)
    {
        LayoutUpdated -= OnFirstLayout;
        CenterViewOnSpawnNode();
    }

    private void CenterViewOnSpawnNode()
    {
        if (_spawner?.RootNode == null) return;

        var view = _spawner.GetViewForNode(_spawner.RootNode);
        if (view == null) return;

        var left = Canvas.GetLeft(view);
        var top = Canvas.GetTop(view);

        var nodeCenter = new Point(left + view.Width / 2, top + view.Height / 2);

        var viewportCenter = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var newX = viewportCenter.X - nodeCenter.X;
        var newY = viewportCenter.Y - nodeCenter.Y;

        _transform.X = newX;
        _transform.Y = newY;
        _gridBackground?.SetOffset(newX, newY);
    }

    private void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
        {
            _isPanning = true;
            _lastMousePosition = e.GetPosition(this);
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    private void CanvasPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPanning) return;

        var current = e.GetPosition(this);
        var deltaX = current.X - _lastMousePosition.X;
        var deltaY = current.Y - _lastMousePosition.Y;

        _transform.X += deltaX;
        _transform.Y += deltaY;
        _gridBackground?.SetOffset(_transform.X, _transform.Y);

        _lastMousePosition = current;
        e.Handled = true;
    }

    private void CanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPanning = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void ClearPath()
    {
        if (_pathPolyline != null && _canvasDrawingArea != null)
        {
            _canvasDrawingArea.Children.Remove(_pathPolyline);
            _pathPolyline = null;
        }
    }

    private void DrawPath(Node goalNode)
    {
        if (_canvasDrawingArea == null || _spawner == null) return;

        var path = BuildPathFromGoal(goalNode);
        if (path.Count < 2) return;

        var points = GetCanvasPoints(path);
        var polyline = new Polyline
        {
            Points = points,
            Stroke = Brushes.Red,
            StrokeThickness = 5,
            ZIndex = 1000
        };

        ClearPath();
        _canvasDrawingArea.Children.Add(polyline);
        _pathPolyline = polyline;

        LogPathToConsole(path);
    }

    private List<Node> BuildPathFromGoal(Node goalNode)
    {
        var path = new List<Node>();
        var current = goalNode;
        while (current != null)
        {
            path.Add(current);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }

    private Points GetCanvasPoints(List<Node> path)
    {
        var points = new Points();
        foreach (var node in path)
        {
            var view = _spawner!.GetViewForNode(node);
            if (view != null)
            {
                double centerX = Canvas.GetLeft(view) + view.Width / 2;
                double centerY = Canvas.GetTop(view) + view.Height / 2;
                points.Add(new Point(centerX, centerY));
            }
        }
        return points;
    }

    private void LogPathToConsole(List<Node> path)
    {
        var nodeIds = new List<string>();
        foreach (var node in path)
        {
            nodeIds.Add(node.Id);
        }
        ConsoleService.Output($"Ruta encontrada: {string.Join(" -> ", nodeIds)}");
    }

    private void OnGoalReached(Node goalNode)
    {
        Dispatcher.UIThread.Post(() =>
        {
            DrawPath(goalNode);
        });
    }
}