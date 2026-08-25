using System.Windows.Threading;
using SmartTaskbar.Core.Classification;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Core.Models;
using SmartTaskbar.Windows.WindowManager;
using SmartTaskbar.Windows.WindowEvents;

namespace SmartTaskbar.App.Services;

/// <summary>
/// Liga a camada de eventos nativos (<see cref="WindowEventWatcher"/>) à classificação de
/// janelas (<see cref="WindowClassifier"/>), produzindo o agrupamento janela → contexto que a
/// UI consome. Eventos chegam em rajada (ex.: várias janelas mudando ao mesmo tempo) — um
/// debounce de 150ms evita reenumerar/reclassificar tudo a cada evento individual.
/// </summary>
public sealed class TaskbarOrchestrator
{
    private static readonly TimeSpan DebounceInterval = TimeSpan.FromMilliseconds(150);

    private readonly WindowEventWatcher _watcher;
    private readonly WindowEnumerator _enumerator;
    private readonly WindowClassifier _classifier;
    private readonly Serilog.ILogger _log;
    private readonly DispatcherTimer _debounceTimer;

    public event EventHandler? Refreshed;

    public IReadOnlyDictionary<Guid, List<WindowInfo>> WindowsByContext { get; private set; } =
        new Dictionary<Guid, List<WindowInfo>>();

    public TaskbarOrchestrator(
        WindowEventWatcher watcher,
        WindowEnumerator enumerator,
        WindowClassifier classifier,
        Serilog.ILogger log)
    {
        _watcher = watcher;
        _enumerator = enumerator;
        _classifier = classifier;
        _log = log.ForContext<TaskbarOrchestrator>();

        _debounceTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = DebounceInterval };
        _debounceTimer.Tick += (_, _) =>
        {
            _debounceTimer.Stop();
            RefreshNow();
        };
    }

    public void Start()
    {
        _watcher.WindowChanged += OnWindowChanged;
        _watcher.Start();
        RefreshNow();
    }

    public void Stop()
    {
        _watcher.WindowChanged -= OnWindowChanged;
        _watcher.Stop();
    }

    /// <summary>Reenumera e reclassifica imediatamente — usado no startup e após editar regras/contextos.</summary>
    public void RefreshNow()
    {
        try
        {
            var windows = _enumerator.EnumerateApplicationWindows();
            var byContext = windows
                .GroupBy(w => _classifier.Classify(w))
                .ToDictionary(g => g.Key, g => g.ToList());

            WindowsByContext = byContext;
            Refreshed?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Falha ao reenumerar/reclassificar janelas");
        }
    }

    private void OnWindowChanged(object? sender, WindowChangedEventArgs e)
    {
        // Evento chega na thread do watcher; DispatcherTimer exige a thread de UI.
        _debounceTimer.Dispatcher.BeginInvoke(() =>
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        });
    }
}
