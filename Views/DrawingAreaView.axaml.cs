using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Digraphia.Algorithms;
using Digraphia.EventManager;
using Digraphia.Services;
using System;
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

        MainEventManager.ModeChanged += OnModeChanged;
    }

    private async void OnSearchStarted(float speed)
    {
        if (_spawner == null) return;

        // Detener búsqueda anterior si existe
        OnSearchStopped();

        var root = _spawner.RootNode;
        if (root == null)
        {
            ConsoleService.Output("No hay nodos en el grafo para iniciar búsqueda.");
            return;
        }

        // Resetear colores de todos los nodos
        foreach (var node in _spawner.AllNodes)
        {
            node.State = NodeState.Normal;
        }

        var algorithm = new DepthFirstSearch();
        _currentRunner = new GraphRunner(algorithm);
        _currentCts = new CancellationTokenSource();

        try
        {
            await _currentRunner.RunAsync(root, speed, _currentCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Cancelación esperada
        }
        finally
        {
            _currentRunner = null;
            _currentCts?.Dispose();
            _currentCts = null;
        }
    }

    private void OnSearchStopped()
    {
        _currentCts?.Cancel();
    }
    private void OnModeChanged(EditorMode mode)
    {
        switch (mode)
        {
            case EditorMode.BuildNode:
                _spawner?.EnableBuildMode();
                break;
            case EditorMode.RemoveNode:
                _spawner?.EnableRemoveMode();
                break;
            default:
                _spawner?.DisableAllModes();
                break;
        }
    }

    private bool _layoutInitialized = false;
    private void OnFirstLayout(object? sender, EventArgs e)
    {
        if (_layoutInitialized || Bounds.Width == 0) return;
        _layoutInitialized = true;

        if (_spawner?.RootNode != null)
        {
            CenterOnNode(_spawner.RootNode);
        }
        else
        {
            _gridBackground?.SetOffset(0, 0);
        }
    }

    public void CenterOnNode(Node node)
    {
        if (_canvasDrawingArea == null || _gridBackground == null || _spawner == null)
            return;

        var view = _spawner.GetViewForNode(node);
        if (view == null) return;

        var left = Canvas.GetLeft(view);
        var top = Canvas.GetTop(view);
        var nodeCenter = new Point(left + view.Width / 2, top + view.Height / 2);

        var viewportCenter = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var newX = viewportCenter.X - nodeCenter.X;
        var newY = viewportCenter.Y - nodeCenter.Y;

        _transform.X = newX;
        _transform.Y = newY;
        _gridBackground.SetOffset(newX, newY);
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
        if (_isPanning && e.InitialPressMouseButton == MouseButton.Middle)
        {
            _isPanning = false;
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }
}