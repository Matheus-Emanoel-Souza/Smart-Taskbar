using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartTaskbar.App.Models;
using SmartTaskbar.App.Services;
using SmartTaskbar.App.Views;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Core.Rules;
using SmartTaskbar.Infrastructure.Settings;

namespace SmartTaskbar.App.ViewModels;

/// <summary>ViewModel da barra flutuante: lista de contextos com contagem de janelas.</summary>
public sealed partial class BarViewModel : ObservableObject
{
    private readonly TaskbarOrchestrator _orchestrator;
    private readonly ContextManager _contexts;
    private readonly RuleManager _rules;
    private readonly WindowAssignmentManager _assignments;
    private readonly SettingsService _settings;

    public ObservableCollection<ContextViewModel> Contexts { get; } = [];

    /// <summary>Disparado quando o usuário pede para abrir o painel de um contexto (clique na pílula).</summary>
    public event EventHandler<Guid>? OpenPanelRequested;

    public IRelayCommand<Guid> OpenPanelCommand { get; }
    public IRelayCommand AddContextCommand { get; }
    public IRelayCommand ToggleExpandCommand { get; }
    public IRelayCommand<BarDockMode> SetDockModeCommand { get; }

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private BarDockMode _dockMode;

    public BarViewModel(
        TaskbarOrchestrator orchestrator,
        ContextManager contexts,
        RuleManager rules,
        WindowAssignmentManager assignments,
        SettingsService settings)
    {
        _orchestrator = orchestrator;
        _contexts = contexts;
        _rules = rules;
        _assignments = assignments;
        _settings = settings;

        var saved = _settings.Load();
        _isExpanded = saved.BarExpanded;
        _dockMode = Enum.TryParse<BarDockMode>(saved.BarDock, out var parsed) ? parsed : BarDockMode.Free;

        OpenPanelCommand = new RelayCommand<Guid>(id => OpenPanelRequested?.Invoke(this, id));
        AddContextCommand = new RelayCommand(AddContext);
        ToggleExpandCommand = new RelayCommand(ToggleExpand);
        SetDockModeCommand = new RelayCommand<BarDockMode>(SetDockMode);

        _orchestrator.Refreshed += (_, _) => Rebuild();
        Rebuild();
    }

    private void ToggleExpand()
    {
        IsExpanded = !IsExpanded;

        var settings = _settings.Load();
        settings.BarExpanded = IsExpanded;
        _settings.Save(settings);
    }

    private void SetDockMode(BarDockMode mode)
    {
        DockMode = mode;

        var settings = _settings.Load();
        settings.BarDock = mode.ToString();
        _settings.Save(settings);
    }

    public void Rebuild()
    {
        var counts = _orchestrator.WindowsByContext;
        var ordered = _contexts.All;

        Contexts.Clear();
        foreach (var context in ordered)
        {
            var count = counts.TryGetValue(context.Id, out var list) ? list.Count : 0;
            Contexts.Add(new ContextViewModel(context, count, _contexts, _rules, _assignments, Rebuild));
        }
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
}
