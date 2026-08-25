using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using SmartTaskbar.App.Services;
using SmartTaskbar.App.Views;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Core.Models;
using SmartTaskbar.Core.Rules;
using SmartTaskbar.Windows.WindowManager;

namespace SmartTaskbar.App.ViewModels;

/// <summary>
/// ViewModel do gerenciador em tela cheia: todos os contextos lado a lado como colunas
/// (como pastas num gerenciador de arquivos), cada um com suas janelas — permite arrastar
/// uma janela de uma coluna para outra sem precisar abrir um painel de cada vez.
/// </summary>
public sealed class ManagerViewModel : IDisposable
{
    private readonly TaskbarOrchestrator _orchestrator;
    private readonly ContextManager _contexts;
    private readonly RuleManager _rules;
    private readonly WindowAssignmentManager _assignments;
    private readonly WindowActivator _activator;
    private readonly WindowIconProvider _icons;

    public ObservableCollection<ManagerColumnViewModel> Columns { get; } = [];
    public IRelayCommand AddContextCommand { get; }

    public ManagerViewModel(
        TaskbarOrchestrator orchestrator,
        ContextManager contexts,
        RuleManager rules,
        WindowAssignmentManager assignments,
        WindowActivator activator,
        WindowIconProvider icons)
    {
        _orchestrator = orchestrator;
        _contexts = contexts;
        _rules = rules;
        _assignments = assignments;
        _activator = activator;
        _icons = icons;

        AddContextCommand = new RelayCommand(AddContext);

        _orchestrator.Refreshed += OnRefreshed;
        Rebuild();
    }

    private void OnRefreshed(object? sender, EventArgs e) => Rebuild();

    private void Rebuild()
    {
        var counts = _orchestrator.WindowsByContext;

        Columns.Clear();
        foreach (var context in _contexts.All)
        {
            var headerCount = counts.TryGetValue(context.Id, out var windows) ? windows.Count : 0;
            var header = new ContextViewModel(context, headerCount, _contexts, _rules, _assignments, Rebuild);
            var column = new ManagerColumnViewModel { Header = header };

            if (windows is not null)
            {
                foreach (var window in windows.OrderBy(w => w.ProcessName).ThenBy(w => w.Title))
                {
                    var iconBytes = _icons.GetIconPng(window.ExecutablePath);
                    column.Windows.Add(new WindowItemViewModel(
                        window, context.Id, iconBytes, _contexts, _assignments, _activator,
                        RequestNewContext, () => _orchestrator.RefreshNow()));
                }
            }

            Columns.Add(column);
        }
    }

    private Context? RequestNewContext()
    {
        var result = ContextEditWindow.Prompt("Novo contexto", string.Empty, "🗂️");
        return result is null ? null : _contexts.Create(result.Value.Name, result.Value.Icon);
    }

    private void AddContext()
    {
        var result = ContextEditWindow.Prompt("Novo contexto", string.Empty, "🗂️");
        if (result is null)
        {
            return;
        }

        _contexts.Create(result.Value.Name, result.Value.Icon);
        Rebuild();
    }

    public void Dispose() => _orchestrator.Refreshed -= OnRefreshed;
}
