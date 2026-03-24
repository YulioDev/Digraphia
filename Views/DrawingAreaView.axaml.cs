using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Digraphia.EventManager;

namespace Digraphia.Views;

public partial class DrawingAreaView : UserControl
{
    private bool _isPanning;
    private Point _lastMousePosition;
    private Canvas? _canvasDrawingArea;
    private GridBackgroundControl? _gridBackground;
    private TranslateTransform _transform = new();
    private NodeSpawner? _spawner;

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

        // Reacciona al modo global
        MainEventManager.ModeChanged += OnModeChanged;

        // Centrar cámara cuando se cree el primer nodo (lo hacemos en el evento de cambio de nodos)
        // Para esto, necesitamos que NodeSpawner notifique cuando se agregue el primer nodo
        // Alternativa: observar el spawner después de que se cree la raíz
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

        // Centrar la vista en el nodo raíz si existe
        if (_spawner?.RootNode != null)
        {
            CenterOnNode(_spawner.RootNode);
        }
        else
        {
            _gridBackground?.SetOffset(0, 0);
        }
    }

    public void CenterOnNode(NodeView node)
    {
        if (_canvasDrawingArea == null || _gridBackground == null) return;

        var left = Canvas.GetLeft(node);
        var top = Canvas.GetTop(node);
        var nodeCenter = new Point(left + node.Width / 2, top + node.Height / 2);

        // Calcular offset para centrar el nodo en el viewport
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