using Avalonia.Threading;
using Digraphia.Algorithms;
using Digraphia.Services;
using Digraphia.Views;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Digraphia;

public class GraphRunner
{
    private readonly IGraphAlgorithm<Node> _algorithm;
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public event Action<Node>? NodeVisited;      // Nodo visitado (resaltado)
    public event Action<Node>? NodeBacktracked;  // Nodo completado (backtrack)

    public GraphRunner(IGraphAlgorithm<Node> algorithm)
    {
        _algorithm = algorithm;
    }

    public async Task RunAsync(Node start, float speed = 0.5f, CancellationToken cancellationToken = default)
    {
        if (_isRunning) return;

        _isRunning = true;
        _algorithm.Initialize(start);

        // Calcular delay en milisegundos (más lento si speed es bajo, más rápido si es alto)
        int delayMs = (int)(1000 / speed); // speed 0.5 -> 2000ms, speed 1 -> 1000ms

        // Limpiar estados previos
        foreach (var node in GetAllNodes(start))
        {
            if (node.State != NodeState.Highlight && node.State != NodeState.Completed)
                node.State = NodeState.Normal;
        }

        ConsoleService.Output($"Iniciando búsqueda DFS desde nodo {start.Id}");
        ConsoleService.State("Ejecutando", ConsoleState.Warning);

        while (!_algorithm.IsFinished && !cancellationToken.IsCancellationRequested)
        {
            var current = _algorithm.GetCurrentNode();
            bool stepResult = _algorithm.Step();

            if (_algorithm.GetCurrentNode() != current)
            {
                // Se avanzó a un nuevo nodo
                var newNode = _algorithm.GetCurrentNode();
                if (newNode != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        newNode.State = NodeState.Highlight;
                        NodeVisited?.Invoke(newNode);
                    });

                    ConsoleService.Output($"Visitando nodo {newNode.Id}");

                    // Verificar si es meta
                    if (newNode.IsGoal)
                    {
                        ConsoleService.Output($"¡Meta alcanzada en nodo {newNode.Id}!");
                        ConsoleService.State("Meta alcanzada", ConsoleState.Ready);
                        break;
                    }

                    await Task.Delay(delayMs, cancellationToken);
                }
            }
            else if (!stepResult)
            {
                // No se avanzó y terminó -> final
                break;
            }
            else
            {
                // Se está procesando backtracking? El algoritmo no expone directamente cuándo retrocede.
                // Podemos detectar si el nodo actual ya fue visitado y se está cerrando.
                // En DFS, al terminar de explorar un nodo, se marcará como Completed.
                // Para ello, en el algoritmo, después de procesar los hijos, podemos marcar el nodo actual como Completed.
                // Modificaremos DepthFirstSearch para que cuando se termine de explorar un nodo (cuando se extrae de la pila y ya fue visitado), lo marque como Completed.
                // Por ahora, asumimos que al no haber nuevo nodo, se está retrocediendo.
                if (_algorithm.GetCurrentNode() != null && _algorithm.GetCurrentNode()!.State == NodeState.Highlight)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _algorithm.GetCurrentNode()!.State = NodeState.Completed;
                        NodeBacktracked?.Invoke(_algorithm.GetCurrentNode()!);
                    });
                    ConsoleService.Output($"Retrocediendo desde nodo {_algorithm.GetCurrentNode()!.Id}");
                    await Task.Delay(delayMs, cancellationToken);
                }
            }
        }

        _isRunning = false;
        if (cancellationToken.IsCancellationRequested)
        {
            ConsoleService.Output("Búsqueda detenida por el usuario.");
            ConsoleService.State("Detenido", ConsoleState.Ready);
        }
        else if (_algorithm.IsFinished)
        {
            ConsoleService.Output("Búsqueda completada (todos los nodos visitados).");
            ConsoleService.State("Completado", ConsoleState.Ready);
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private IEnumerable<Node> GetAllNodes(Node start)
    {
        // Obtener todos los nodos a través del árbol (necesitamos acceso al árbol)
        // Podríamos pasar el árbol al runner o acceder desde el spawner.
        // Simplificamos: el runner no necesita esta funcionalidad para resetear colores,
        // porque la limpieza se hará desde el spawner antes de iniciar.
        yield break;
    }
}