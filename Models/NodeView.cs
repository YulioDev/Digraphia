using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace Digrafos.Views;

public class NodeView : UserControl
{
    private readonly Ellipse _circle;
    private readonly TextBlock _label;

    public string NodeText
    {
        get => _label.Text ?? "";
        set => _label.Text = value;
    }

    public bool IsAvailableSlot { get; private set; }
    public NodeView? ParentNode { get; set; }

    // Nueva propiedad: lista de nodos hijos (adyacentes en el grafo)
    public List<NodeView> Children { get; } = new List<NodeView>();

    public NodeView(string content = "", bool isAvailableSlot = false)
    {
        IsAvailableSlot = isAvailableSlot;
        Width = 40;
        Height = 40;

        _circle = new Ellipse
        {
            Width = 40,
            Height = 40,
            StrokeThickness = 2
        };

        if (IsAvailableSlot)
        {
            _circle.Fill = new SolidColorBrush(Color.Parse("#80FFA500"));
            _circle.Stroke = new SolidColorBrush(Color.Parse("#FFA500"));
        }
        else
        {
            _circle.Fill = new SolidColorBrush(Color.Parse("#3C3C3C"));
            _circle.Stroke = new SolidColorBrush(Color.Parse("#4D78A8"));
        }

        _label = new TextBlock
        {
            Text = content,
            Foreground = Brushes.White,
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        var grid = new Grid();
        grid.Children.Add(_circle);
        grid.Children.Add(_label);
        
        Content = grid;
    }

    public void ConvertToRealNode(string content)
    {
        IsAvailableSlot = false;
        NodeText = content;
        
        _circle.Fill = new SolidColorBrush(Color.Parse("#3C3C3C"));
        _circle.Stroke = new SolidColorBrush(Color.Parse("#4D78A8"));
    }

    // Nuevo método: cambiar colores de relleno y borde
    public void ChangeColor(string fillHex, string strokeHex)
    {
        _circle.Fill = new SolidColorBrush(Color.Parse(fillHex));
        _circle.Stroke = new SolidColorBrush(Color.Parse(strokeHex));
    }
}

public static class ConnectionBuilder
{
    public static void DrawDirectedConnection(Canvas canvas, Control parent, Control child)
    {
        double radius = parent.Width / 2;
        
        double x1 = Canvas.GetLeft(parent) + radius;
        double y1 = Canvas.GetTop(parent) + radius;
        double x2 = Canvas.GetLeft(child) + radius;
        double y2 = Canvas.GetTop(child) + radius;

        double dx = x2 - x1;
        double dy = y2 - y1;
        double angle = Math.Atan2(dy, dx);

        double startX = x1 + radius * Math.Cos(angle);
        double startY = y1 + radius * Math.Sin(angle);

        double endX = x2 - radius * Math.Cos(angle);
        double endY = y2 - radius * Math.Sin(angle);

        var line = new Line
        {
            StartPoint = new Avalonia.Point(startX, startY),
            EndPoint = new Avalonia.Point(endX, endY),
            Stroke = Brushes.Gray,
            StrokeThickness = 2,
            ZIndex = -1
        };

        canvas.Children.Add(line);

        double arrowSize = 12;
        double arrowAngle = Math.PI / 6;

        var p1 = new Avalonia.Point(endX, endY);
        var p2 = new Avalonia.Point(
            endX - arrowSize * Math.Cos(angle - arrowAngle),
            endY - arrowSize * Math.Sin(angle - arrowAngle));
        var p3 = new Avalonia.Point(
            endX - arrowSize * Math.Cos(angle + arrowAngle),
            endY - arrowSize * Math.Sin(angle + arrowAngle));

        var arrowHead = new Polygon
        {
            Points = new List<Avalonia.Point> { p1, p2, p3 },
            Fill = Brushes.Gray,
            ZIndex = -1
        };

        canvas.Children.Add(arrowHead);
    }
}