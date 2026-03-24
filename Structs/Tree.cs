using System.Collections.Generic;
using System.Linq;

namespace Digraphia;

public class Tree
{
    public Node? Root { get; private set; }
    public List<Node> AllNodes { get; private set; }
    public List<Node> PendingDeletion { get; private set; }

    // Constructor sin parámetros que inicializa las listas
    public Tree()
    {
        AllNodes = new List<Node>();
        PendingDeletion = new List<Node>();
    }

    public Tree(Node? root = null) : this()
    {
        Root = root;
        if (root != null)
            AddNode(root);
    }

    public void AddNode(Node node)
    {
        if (AllNodes.Contains(node))
            return;

        AllNodes.Add(node);

        if (node.Parent == null && node != Root)
        {
            if (!PendingDeletion.Contains(node))
                PendingDeletion.Add(node);
        }
        else
        {
            PendingDeletion.Remove(node);
        }

        if (Root == null && node.Parent == null)
            Root = node;
    }

    public void RemoveNode(Node node)
    {
        if (!AllNodes.Contains(node))
            return;

        if (node.Parent != null)
            node.Parent.Children.Remove(node);

        foreach (var child in node.Children.ToList())
            child.Parent = null;

        node.Children.Clear();

        AllNodes.Remove(node);
        PendingDeletion.Remove(node);

        if (node == Root)
            Root = null;
    }

    public void ClearPendingDeletion() => PendingDeletion.Clear();

    public Node? FindNearestNode(Vector2 targetPosition)
    {
        return AllNodes
            .OrderBy(n => n.GridPosition.DistanceSquaredTo(targetPosition))
            .FirstOrDefault();
    }

    public IEnumerable<Node> GetAllNodes() => AllNodes;
}