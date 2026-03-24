using Avalonia.Threading;
using Digraphia.Algorithms;
using Digraphia.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Digraphia;

public class GraphRunner
{
    private readonly IGraphAlgorithm<Node> _algorithm;
    private bool _isRunning;

    public event Action<Node>? NodeVisited;

    public GraphRunner(IGraphAlgorithm<Node> algorithm)
    {
        _algorithm = algorithm;
    }

    public async Task RunAsync(Node start, float speed = 0.5f, CancellationToken cancellationToken = default)
    {
        if (_isRunning) { return; }

        _isRunning = true;
        _algorithm.Initialize(start);

        int delayMs = (int)(1000 / speed);
        delayMs = Math.Max(10, Math.Min(2000, delayMs));

        ConsoleService.Output($"Iniciando búsqueda DFS desde nodo {start.Id} (delay {delayMs}ms)");
        ConsoleService.State("Ejecutando", ConsoleState.Warning);

        try
        {
            while (!_algorithm.IsFinished && !cancellationToken.IsCancellationRequested)
            {
                bool stepResult = _algorithm.Step();
                var newCurrent = _algorithm.GetCurrentNode();

                if (stepResult && newCurrent != null)
                {
                    NodeVisited?.Invoke(newCurrent);
                    ConsoleService.Output($"Visitando nodo {newCurrent.Id}");

                    if (newCurrent.IsGoal)
                    {
                        ConsoleService.Output($"¡Meta alcanzada en nodo {newCurrent.Id}!");
                        ConsoleService.State("Meta alcanzada", ConsoleState.Ready);

                        Dispatcher.UIThread.Post(() => newCurrent.State = NodeState.Selected);

                        // Backtracking: Recorre y colorea de azul la ruta conectada hasta el nodo raíz
                        var pathNode = newCurrent.Parent;
                        while (pathNode != null)
                        {
                            var capture = pathNode;
                            Dispatcher.UIThread.Post(() => capture.State = NodeState.Path);
                            pathNode = pathNode.Parent;
                        }

                        if (Globals.Instance.StopAtGoal) { break; }
                    }

                    await Task.Delay(delayMs, cancellationToken);
                }
                else if (!stepResult)
                {
                    break;
                }
                else
                {
                    await Task.Delay(10, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ConsoleService.Output($"Error durante la búsqueda: {ex.Message}");
            ConsoleService.State("Error", ConsoleState.Error);
        }
        finally
        {
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
    }
}