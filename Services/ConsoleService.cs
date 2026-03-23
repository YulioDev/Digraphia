using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Digraphia.Services;

public enum ConsoleState
{
    Ready  = 1, // Verde
    Warning = 2, // Ámbar
    Error  = 3  // Rojo
}

public static class ConsoleService
{
    // Evento al que la vista se suscribe
    public static event Action<string>? MessageLogged;
    public static event Action<string, ConsoleState>? StateChanged;

    private static readonly ConcurrentQueue<Func<Task>> _jobs = new();
    private static readonly SemaphoreSlim _signal = new(0);
    private static string _currentState = "● Listo";
    private static ConsoleState _currentMode = ConsoleState.Ready;

    static ConsoleService()
    {
        // Worker dedicado que procesa jobs en background
        Task.Run(ProcessQueueAsync);
    }

    // Envía un mensaje a la consola
    public static void Output(string message)
    {
        EnqueueJob(() =>
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            MessageLogged?.Invoke(line);
            return Task.CompletedTask;
        });
    }

    // Cambia el estado/luz del indicador
    public static void State(string text, ConsoleState mode)
    {
        EnqueueJob(() =>
        {
            _currentState = mode switch
            {
                ConsoleState.Ready   => $"● {text}",
                ConsoleState.Warning => $"◉ {text}",
                ConsoleState.Error   => $"✕ {text}",
                _                   => $"● {text}"
            };
            _currentMode = mode;
            StateChanged?.Invoke(_currentState, mode);
            return Task.CompletedTask;
        });
    }

    private static void EnqueueJob(Func<Task> job)
    {
        _jobs.Enqueue(job);
        _signal.Release();
    }

    private static async Task ProcessQueueAsync()
    {
        while (true)
        {
            await _signal.WaitAsync();
            if (_jobs.TryDequeue(out var job))
            {
                try   { await job(); }
                catch { /* silencioso, no rompe el worker */ }
            }
        }
    }
}       