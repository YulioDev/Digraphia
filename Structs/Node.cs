using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Digraphia;

public enum NodeState
{
    Normal,
    Highlight,
    Completed,
    Selected
}

public class Node : INotifyPropertyChanged
{
    private string _id = null!;  // inicializado en constructor
    private Vector2 _gridPosition;
    private Node? _parent;
    private List<Node> _children = new();
    private NodeState _state = NodeState.Normal;

    public string Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public Vector2 GridPosition
    {
        get => _gridPosition;
        set => SetField(ref _gridPosition, value);
    }

    public Node? Parent
    {
        get => _parent;
        set => SetField(ref _parent, value);
    }

    public List<Node> Children
    {
        get => _children;
        set => SetField(ref _children, value);
    }

    public NodeState State
    {
        get => _state;
        set => SetField(ref _state, value);
    }

    public Node(string id, Vector2 gridPosition)
    {
        Id = id;
        GridPosition = gridPosition;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // Dentro de la clase Node, agregar:
private bool _isGoal;

    public bool IsGoal
    {
        get => _isGoal;
        set => SetField(ref _isGoal, value);
    }
}