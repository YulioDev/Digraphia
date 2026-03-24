using Digraphia.Views;

namespace Digraphia.Algorithms;

/// <summary>
/// Interfaz general para algoritmos de recorrido de grafos.
/// </summary>
public interface IGraphAlgorithm
{
    /// <summary>
    /// Inicializa el algoritmo desde un nodo de inicio.
    /// </summary>
    void Initialize(NodeView start);

    /// <summary>
    /// Avanza un paso en el recorrido.
    /// </summary>
    /// <returns>True si aún hay pasos pendientes, False si terminó.</returns>
    bool Step();

    /// <summary>
    /// Indica si el recorrido ha finalizado.
    /// </summary>
    bool IsFinished { get; }

    /// <summary>
    /// Obtiene el nodo actualmente visitado.
    /// </summary>
    NodeView? GetCurrentNode();

    /// <summary>
    /// Restablece el estado del algoritmo para poder ejecutarlo nuevamente.
    /// </summary>
    void Reset();
}