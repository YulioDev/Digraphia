using System;

namespace Digraphia.EventManager;

public static class MainEventManager
{
    public static event Action<EditorMode>? ModeChanged;

    public static event Action<float>? SearchStarted;
    public static event Action? SearchStopped;

    public static void SetMode(EditorMode mode)
    {
        Globals.Instance.CurrentMode = mode;
        ModeChanged?.Invoke(mode);
    }

    public static void StartSearch(float speed)
    {
        SearchStarted?.Invoke(speed);
    }

    public static void StopSearch()
    {
        SearchStopped?.Invoke();
    }
}