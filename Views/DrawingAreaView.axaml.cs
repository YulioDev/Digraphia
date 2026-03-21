using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Digrafos.EventManager;

namespace Digrafos.Views;

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
        _gridBackground    = this.FindControl<GridBackgroundControl>("gridBackground");

        if (_canvasDrawingArea != null && _gridBackground != null)
        {
            _canvasDrawingArea.RenderTransform = _transform;
            _spawner = new NodeSpawner(_canvasDrawingArea, _gridBackground);
        }

        PointerPressed     += Canvas_PointerPressed;
        PointerMoved       += CanvasPointerMoved;
        PointerReleased    += CanvasPointerReleased;
        PointerCaptureLost += (_, _) => _isPanning = false;
        LayoutUpdated      += OnFirstLayout;

        // Reacciona al modo global
        MainEventManager.ModeChanged += OnModeChanged;

        // ===== TESTING — BORRAR =====
float spacing = 2f;
var   origin  = new Vector2(0, 0);

// Punto raíz
_gridBackground?.AddPoint(origin, Colors.Cyan);

// Fila 1: punto central justo abajo
var center = origin + new Vector2(0, spacing);
_gridBackground?.AddPoint(center, Colors.Orange);

// Fila 2: dos puntos equidistantes alrededor del centro
var left  = center + new Vector2(-spacing, 0);
var right = center + new Vector2( spacing, 0);
_gridBackground?.AddPoint(left,  Colors.Orange);
_gridBackground?.AddPoint(right, Colors.Orange);
// ===== FIN TESTING ==========
    }

    private void OnModeChanged(EditorMode mode)
    {
        if (mode == EditorMode.BuildNode)
            _spawner?.ListenForPlacement();
    }

    private bool _layoutInitialized = false;
    private void OnFirstLayout(object? sender, EventArgs e)
    {
        if (_layoutInitialized || Bounds.Width == 0) return;
        _layoutInitialized = true;
        _gridBackground?.SetOffset(0, 0);
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
        var deltaX  = current.X - _lastMousePosition.X;
        var deltaY  = current.Y - _lastMousePosition.Y;

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