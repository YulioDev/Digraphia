/* Archivo: Views/ConfigurationView.axaml.cs */
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Digrafos.EventManager;
using Digrafos.Services;

namespace Digrafos.Views;

public partial class ConfigurationView : UserControl
{
    private bool _isAddingNodes = false;

    public ConfigurationView()
    {
        InitializeComponent();

        btnAddNode.Click += (_, _) => ToggleAddNodeMode();
        btnRemoveNode.Click += (_, _) => MainEventManager.SetMode(EditorMode.RemoveNode);
        
        btnStartSearch.Click += (_, _) => OnStartSearch();
        btnStopSearch.Click += (_, _) => OnStopSearch();
        
        sldSpeed.PropertyChanged += (_, e) =>
        {
            if (e.Property == RangeBase.ValueProperty)
                txtSpeedValue.Text = $"{(int)(sldSpeed.Value * 100)}%";
        };
    }

    /* Cambia el estado del boton y emite el evento para que el Spawner (que deberia estar 
       escuchando en el controlador principal) muestre u oculte los slots disponibles */
    private void ToggleAddNodeMode()
    {
        _isAddingNodes = !_isAddingNodes;
        
        if (_isAddingNodes)
        {
            btnAddNode.Content = "Terminar Añadir Nodo";
            btnAddNode.Classes.Add("UnityButtonPrimary"); // Feedback visual de que esta activo
            MainEventManager.SetMode(EditorMode.BuildNode);
        }
        else
        {
            btnAddNode.Content = "Añadir Nodo";
            btnAddNode.Classes.Remove("UnityButtonPrimary");
            MainEventManager.SetMode(EditorMode.NoAction);
        }
    }

    private void OnStartSearch()
    {
        ConsoleService.Output("Búsqueda iniciada.");
        ConsoleService.State("Ejecutando", ConsoleState.Warning);
    }

    private void OnStopSearch()
    {
        ConsoleService.Output("Búsqueda detenida.");
        ConsoleService.State("Listo", ConsoleState.Ready);
    }
}