using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Digraphia.EventManager;
using Digraphia.Services;

namespace Digraphia.Views;

public partial class ConfigurationView : UserControl
{
    private bool _isAddingNodes = false;
    private bool _isRemovingNodes = false;
    private bool _isMarkingGoal = false;

    public ConfigurationView()
    {
        InitializeComponent();

        btnAddNode.Click += (_, _) => ToggleAddNodeMode();
        btnRemoveNode.Click += (_, _) => ToggleRemoveNodeMode();
        btnMarkGoal.Click += (_, _) => ToggleGoalMode();

        btnStartSearch.Click += (_, _) => OnStartSearch();
        btnStopSearch.Click += (_, _) => MainEventManager.StopSearch();

        sldSpeed.PropertyChanged += (_, e) =>
        {
            if (e.Property == RangeBase.ValueProperty) { txtSpeedValue.Text = $"{(int)(sldSpeed.Value * 100)}%"; }
        };
    }

    private void ClearAllModes()
    {
        _isAddingNodes = false;
        _isRemovingNodes = false;
        _isMarkingGoal = false;

        btnAddNode.Classes.Remove("UnityButtonPrimary");
        btnRemoveNode.Classes.Remove("UnityButtonPrimary");
        btnMarkGoal.Classes.Remove("UnityButtonPrimary");
    }

    private void ToggleAddNodeMode()
    {
        bool wasActive = _isAddingNodes;
        ClearAllModes();

        if (!wasActive)
        {
            _isAddingNodes = true;
            btnAddNode.Classes.Add("UnityButtonPrimary");
            MainEventManager.SetMode(EditorMode.BuildNode);
        }
        else { MainEventManager.SetMode(EditorMode.NoAction); }
    }

    private void ToggleRemoveNodeMode()
    {
        bool wasActive = _isRemovingNodes;
        ClearAllModes();

        if (!wasActive)
        {
            _isRemovingNodes = true;
            btnRemoveNode.Classes.Add("UnityButtonPrimary");
            MainEventManager.SetMode(EditorMode.RemoveNode);
        }
        else { MainEventManager.SetMode(EditorMode.NoAction); }
    }

    private void ToggleGoalMode()
    {
        bool wasActive = _isMarkingGoal;
        ClearAllModes();

        if (!wasActive)
        {
            _isMarkingGoal = true;
            btnMarkGoal.Classes.Add("UnityButtonPrimary");
            MainEventManager.SetMode(EditorMode.GoalMode);
        }
        else { MainEventManager.SetMode(EditorMode.NoAction); }
    }

    private void OnStartSearch()
    {
        float speed = (float)sldSpeed.Value;
        ConsoleService.Output($"Iniciando búsqueda desde UI con speed={speed}");
        MainEventManager.StartSearch(speed);
    }
}