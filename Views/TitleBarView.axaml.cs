using Avalonia.Controls;
using Avalonia.Input;

namespace Digrafos.Views;

public partial class TitleBarView : UserControl
{
    public TitleBarView()
    {
        InitializeComponent();
        
        var btnClose = this.FindControl<Button>("btnClose");
        var btnMaximize = this.FindControl<Button>("btnMaximize");
        var btnMinimize = this.FindControl<Button>("btnMinimize");
        var pnlTitleBarDragArea = this.FindControl<Panel>("pnlTitleBarDragArea");

        if (btnClose != null) btnClose.Click += (sender, args) => (VisualRoot as Window)?.Close();
        
        if (btnMaximize != null) btnMaximize.Click += (sender, args) => 
        {
            if (VisualRoot as Window is { } win)
            {
                win.WindowState = win.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
        };
        
        if (btnMinimize != null) btnMinimize.Click += (sender, args) => 
        {
            if (VisualRoot as Window is { } win)
            {
                win.WindowState = WindowState.Minimized;
            }
        };

        if (pnlTitleBarDragArea != null)
        {
            /* Al delegar la vista a un UserControl, ya no tenemos acceso directo a las propiedades de la Window principal. 
            Usar VisualRoot nos permite obtener la ventana padre real en el momento exacto en que el usuario interactua, 
            asegurando que funciones como BeginMoveDrag operen sobre la ventana completa y no sobre el control local.
            */
            pnlTitleBarDragArea.PointerPressed += (sender, args) =>
            {
                if (args.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    (VisualRoot as Window)?.BeginMoveDrag(args);
                }
            };
        }
    }
}