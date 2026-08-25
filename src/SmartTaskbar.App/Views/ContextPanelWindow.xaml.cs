using System.Windows;
using SmartTaskbar.App.ViewModels;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Windows.WindowManager;

namespace SmartTaskbar.App.Views;

/// <summary>Painel com a lista de janelas de um contexto. Fecha ao perder o foco (clique fora).</summary>
public partial class ContextPanelWindow : Window
{
    private readonly ContextPanelViewModel _viewModel;

    public ContextPanelWindow(
        Guid contextId,
        Services.TaskbarOrchestrator orchestrator,
        ContextManager contexts,
        WindowAssignmentManager assignments,
        WindowActivator activator,
        WindowIconProvider icons)
    {
        InitializeComponent();

        _viewModel = new ContextPanelViewModel(contextId, orchestrator, contexts, assignments, activator, icons);
        DataContext = _viewModel;

        Closed += (_, _) => _viewModel.Dispose();
    }

    private void OnDeactivated(object sender, EventArgs e) => Close();
}
