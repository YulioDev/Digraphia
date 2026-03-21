public class GraphRunner
{
    private readonly HashSet<NodeView> _visited = new();
    private readonly int _delayMilliseconds;

    public GraphRunner(int delayMilliseconds = 500)
    {
        _delayMilliseconds = delayMilliseconds;
    }

    public async Task StartDepthFirstSearch(NodeView? rootNode, IEnumerable<NodeView> allNodes)
    {
        if (rootNode == null) return;

        foreach (var node in allNodes)
        {
            node.ChangeColor("#3C3C3C", "#4D78A8");
        }

        _visited.Clear();
        await TraverseDFS(rootNode);
    }

    /*
    Implementamos el recorrido en profundidad mediante recursion asincrona.
    Esto pausa la ejecucion visualmente sin bloquear el hilo principal de la interfaz grafica 
    para que el usuario pueda ver el avance en tiempo real.
    */
    private async Task TraverseDFS(NodeView currentNode)
    {
        if (_visited.Contains(currentNode)) return;

        _visited.Add(currentNode);
        
        currentNode.ChangeColor("#2E7D32", "#4CAF50");
        await Task.Delay(_delayMilliseconds);

        /*
        Al iterar sobre los hijos garantizamos el avance hacia la izquierda primero 
        ya que la lista mantiene el orden natural de creacion de los nodos en la vista.
        */
        foreach (var childNode in currentNode.Children)
        {
            if (!_visited.Contains(childNode))
            {
                await TraverseDFS(childNode);
            }
        }

        currentNode.ChangeColor("#C62828", "#E53935");
        await Task.Delay(_delayMilliseconds);
    }
}