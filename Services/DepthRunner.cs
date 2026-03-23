using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Digraphia.Views;

namespace Digraphia
{
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

            // Restablecer colores de todos los nodos
            foreach (var node in allNodes)
            {
                node.ChangeColor("#3C3C3C", "#4D78A8");
            }

            _visited.Clear();
            await TraverseDFS(rootNode);
        }

        /// <summary>
        /// Recorrido en profundidad recursivo asincrónico con pausas visuales.
        /// </summary>
        private async Task TraverseDFS(NodeView currentNode)
        {
            if (_visited.Contains(currentNode)) return;

            _visited.Add(currentNode);

            // Nodo visitado: color verde
            currentNode.ChangeColor("#2E7D32", "#4CAF50");
            await Task.Delay(_delayMilliseconds);

            // Explorar todos los hijos (adyacentes)
            foreach (var childNode in currentNode.Children)
            {
                if (!_visited.Contains(childNode))
                {
                    await TraverseDFS(childNode);
                }
            }

            // Nodo terminado: color rojo
            currentNode.ChangeColor("#C62828", "#E53935");
            await Task.Delay(_delayMilliseconds);
        }
    }
}