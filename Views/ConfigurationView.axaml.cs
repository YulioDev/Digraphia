/* Archivo: Views/ConfigurationView.axaml.cs */
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Digraphia.EventManager;
using Digraphia.Services;

namespace Digraphia.Views;

public partial class ConfigurationView : UserControl
{
    private bool _isAddingNodes = false;

    public ConfigurationView()
    {
        InitializeComponent();

        btnAddNode.Click += (_, _) => ToggleAddNodeMode();
        btnRemoveNode.Click += (_, _) => ToggleRemoveNodeMode();

        btnStartSearch.Click += (_, _) => OnStartSearch();
        btnStopSearch.Click += (_, _) => OnStopSearch();

        sldSpeed.PropertyChanged += (_, e) =>
        {
            if (e.Property == RangeBase.ValueProperty)
                txtSpeedValue.Text = $"{(int)(sldSpeed.Value * 100)}%";
        };
    }

    private void ToggleAddNodeMode()
    {
        if (_isAddingNodes)
        {
            // Desactivar modo añadir
            _isAddingNodes = false;
            btnAddNode.Content = "Añadir Nodo";
            btnAddNode.Classes.Remove("UnityButtonPrimary");
            MainEventManager.SetMode(EditorMode.NoAction);
        }
        else
        {
            // Activar modo añadir, desactivar modo eliminar si estaba activo
            _isAddingNodes = true;
            btnAddNode.Content = "Terminar Añadir Nodo";
            btnAddNode.Classes.Add("UnityButtonPrimary");
            // Desmarcar botón de eliminar
            btnRemoveNode.Classes.Remove("UnityButtonPrimary");
            MainEventManager.SetMode(EditorMode.BuildNode);
        }
    }

    private void ToggleRemoveNodeMode()
    {
        // Alternar modo eliminar, desactivando modo añadir si estaba activo
        if (_isAddingNodes)
        {
            // Desactivar modo añadir primero
            _isAddingNodes = false;
            btnAddNode.Content = "Añadir Nodo";
            btnAddNode.Classes.Remove("UnityButtonPrimary");
        }
        // Activar modo eliminar (no es toggle, se activa y se desactiva al salir)
        btnRemoveNode.Classes.Add("UnityButtonPrimary");
        MainEventManager.SetMode(EditorMode.RemoveNode);
    }

    // En OnStartSearch() y OnStopSearch():
    private void OnStartSearch()
    {
        float speed = (float)sldSpeed.Value;
        MainEventManager.StartSearch(speed);
    }

    private void OnStopSearch()
    {
        MainEventManager.StopSearch();
    }
}