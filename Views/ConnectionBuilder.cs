using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Digraphia.Views;

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
            StartPoint = new Point(startX, startY),
            EndPoint = new Point(endX, endY),
            Stroke = Brushes.Gray,
            StrokeThickness = 2,
            ZIndex = -1
        };

        canvas.Children.Add(line);

        double arrowSize = 12;
        double arrowAngle = Math.PI / 6;

        var p1 = new Point(endX, endY);
        var p2 = new Point(
            endX - arrowSize * Math.Cos(angle - arrowAngle),
            endY - arrowSize * Math.Sin(angle - arrowAngle));
        var p3 = new Point(
            endX - arrowSize * Math.Cos(angle + arrowAngle),
            endY - arrowSize * Math.Sin(angle + arrowAngle));

        var arrowHead = new Polygon
        {
            Points = new List<Point> { p1, p2, p3 },
            Fill = Brushes.Gray,
            ZIndex = -1
        };

        canvas.Children.Add(arrowHead);
    }
}