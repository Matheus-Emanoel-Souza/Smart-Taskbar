using System.Collections.ObjectModel;
using SmartTaskbar.App.Services;
using SmartTaskbar.App.Views;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Core.Models;
using SmartTaskbar.Windows.WindowManager;
using Context = SmartTaskbar.Core.Models.Context;

namespace SmartTaskbar.App.ViewModels;

/// <summary>
/// ViewModel do painel que lista as janelas de um contexto. Uma instância por abertura do
/// painel; <see cref="Dispose"/> deve ser chamado ao fechar a janela para não acumular
/// handlers em <see cref="TaskbarOrchestrator.Refreshed"/> — o app fica aberto o tempo todo,
/// então esse desinscrever evita um memory/CPU leak lento.
/// </summary>
public sealed class ContextPanelViewModel : IDisposable
{
    private readonly Guid _contextId;
    private readonly TaskbarOrchestrator _orchestrator;
    private readonly ContextManager _contexts;
    private readonly WindowAssignmentManager _assignments;
    private readonly WindowActivator _activator;
    private readonly WindowIconProvider _icons;

    public string ContextName { get; }
    public string ContextIcon { get; }
    public ObservableCollection<WindowItemViewModel> Windows { get; } = [];

    public ContextPanelViewModel(
        Guid contextId,
        TaskbarOrchestrator orchestrator,
        ContextManager contexts,
        WindowAssignmentManager assignments,
        WindowActivator activator,
        WindowIconProvider icons)
    {
        _contextId = contextId;
        _orchestrator = orchestrator;
        _contexts = contexts;
        _assignments = assignments;
        _activator = activator;
        _icons = icons;

        var context = contexts.Find(contextId) ?? Context.CreateUncategorized();
        ContextName = context.Name;
        ContextIcon = context.Icon;

        _orchestrator.Refreshed += OnRefreshed;
        Rebuild();
    }

    private void OnRefreshed(object? sender, EventArgs e) => Rebuild();

    private void Rebuild()
    {
        Windows.Clear();

        if (!_orchestrator.WindowsByContext.TryGetValue(_contextId, out var list))
        {
            return;
        }

        foreach (var window in list.OrderBy(w => w.ProcessName).ThenBy(w => w.Title))
        {
            var iconBytes = _icons.GetIconPng(window.ExecutablePath);
            Windows.Add(new WindowItemViewModel(
                window, _contextId, iconBytes, _contexts, _assignments, _activator,
                RequestNewContext, OnMoved));
        }
    }

    private Context? RequestNewContext()
    {
        var result = ContextEditWindow.Prompt("Novo contexto", string.Empty, "🗂️");
        return result is null ? null : _contexts.Create(result.Value.Name, result.Value.Icon);
    }

    private void OnMoved() => _orchestrator.RefreshNow();

    public void Dispose() => _orchestrator.Refreshed -= OnRefreshed;
}
