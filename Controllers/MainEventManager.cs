using System;

namespace Digraphia.EventManager;

public static class MainEventManager
{
    public static event Action<EditorMode>? ModeChanged;

    public static void SetMode(EditorMode mode)
    {
        Globals.Instance.CurrentMode = mode;
        ModeChanged?.Invoke(mode);
    }
}