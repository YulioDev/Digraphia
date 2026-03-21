using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Digrafos.Views;

public class GridBackgroundControl : Control
{
    private double _offsetX = 0;
    private double _offsetY = 0;
    private const float CellSize = 40f;

    // Puntos registrados para dibujar
    private readonly List<(Vector2 pos, Color color)> _points = new();

    public float GridCellSize => CellSize;

    public void SetOffset(double x, double y)
    {
        _offsetX = x;
        _offsetY = y;
        InvalidateVisual();
    }

    /// Registra un Vector2 para ser dibujado en el grid
    public void AddPoint(Vector2 point, Color color)
    {
        _points.Add((point, color));
        InvalidateVisual();
    }

    public void ClearPoints()
    {
        _points.Clear();
        InvalidateVisual();
    }

    /// Convierte un Vector2 (coordenadas de grilla) a Point en el canvas
    public Point GridToCanvas(Vector2 v) => v.ToCanvasPoint(CellSize);

    public override void Render(DrawingContext context)
    {
        var pen    = new Pen(new SolidColorBrush(Color.Parse("#2A2A2A")), 1);
        var width  = Bounds.Width;
        var height = Bounds.Height;

        // Líneas con offset (se mueven con la cámara)
        for (double x = _offsetX % CellSize; x < width; x += CellSize)
            context.DrawLine(pen, new Point(x, 0), new Point(x, height));

        for (double y = _offsetY % CellSize; y < height; y += CellSize)
            context.DrawLine(pen, new Point(0, y), new Point(width, y));

        // Puntos en coordenadas absolutas + offset de cámara
        foreach (var (vec, color) in _points)
        {
            var brush  = new SolidColorBrush(color);
            var center = new Point(
                vec.X * CellSize + _offsetX,
                vec.Y * CellSize + _offsetY
            );
            context.DrawEllipse(brush, null, center, 4, 4);
        }
    }
}