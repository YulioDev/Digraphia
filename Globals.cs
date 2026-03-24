using System;

namespace Digraphia;

public enum EditorMode
{
    NoAction = 1,
    BuildNode = 2,
    RemoveNode = 3,
    GoalMode = 4
}

public sealed class Globals
{
    private static readonly Lazy<Globals> _instance = new(() => new Globals());
    public static Globals Instance => _instance.Value;

    private Globals() { }

    public EditorMode CurrentMode { get; set; } = EditorMode.NoAction;
    public bool StopAtGoal { get; set; } = true;
}