using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using Digraphia;

namespace Digraphia.Views;

public partial class NodeView : UserControl
{
    public static readonly StyledProperty<Node?> NodeProperty =
        AvaloniaProperty.Register<NodeView, Node?>(nameof(Node));

    public Node? Node
    {
        get => GetValue(NodeProperty);
        set => SetValue(NodeProperty, value);
    }

    public NodeView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        // Si el DataContext es un Node, lo asignamos a la propiedad Node.
        if (DataContext is Node node)
            Node = node;
    }
}

// Convertidores para los pinceles según el estado
public class StateToFillBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NodeState state)
        {
            return state switch
            {
                NodeState.Normal => new SolidColorBrush(Color.Parse("#3C3C3C")),
                NodeState.Highlight => new SolidColorBrush(Color.Parse("#2E7D32")),
                NodeState.Completed => new SolidColorBrush(Color.Parse("#C62828")),
                NodeState.Selected => new SolidColorBrush(Color.Parse("#FFA500")),
                _ => new SolidColorBrush(Color.Parse("#3C3C3C"))
            };
        }
        return new SolidColorBrush(Color.Parse("#3C3C3C"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class StateToStrokeBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NodeState state)
        {
            return state switch
            {
                NodeState.Normal => new SolidColorBrush(Color.Parse("#4D78A8")),
                NodeState.Highlight => new SolidColorBrush(Color.Parse("#4CAF50")),
                NodeState.Completed => new SolidColorBrush(Color.Parse("#E53935")),
                NodeState.Selected => new SolidColorBrush(Color.Parse("#FFA500")),
                _ => new SolidColorBrush(Color.Parse("#4D78A8"))
            };
        }
        return new SolidColorBrush(Color.Parse("#4D78A8"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}