using System.Windows;
using System.Windows.Input;
using SmartTaskbar.App.Services;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Windows.WindowManager;

namespace SmartTaskbar.App.Views;

/// <summary>Painel com a lista de janelas de um contexto. Fecha ao perder o foco (clique fora).</summary>
public partial class ContextPanelWindow : Window
{
    private readonly ViewModels.ContextPanelViewModel _viewModel;
    private readonly WindowDragSource _dragSource = new();

    private bool _isDragging;

    public ContextPanelWindow(
        Guid contextId,
        Services.TaskbarOrchestrator orchestrator,
        ContextManager contexts,
        WindowAssignmentManager assignments,
        WindowActivator activator,
        WindowIconProvider icons)
    {
        InitializeComponent();

        _viewModel = new ViewModels.ContextPanelViewModel(contextId, orchestrator, contexts, assignments, activator, icons);
        DataContext = _viewModel;

        Closed += (_, _) => _viewModel.Dispose();
    }

    private void OnDeactivated(object sender, EventArgs e)
    {
        // Arrastar uma janela até a barra ativa a janela da barra e desativa este painel —
        // não fechar nesse meio-tempo, senão o drag é cancelado antes de soltar no alvo.
        if (!_isDragging)
        {
            Close();
        }
    }

    private void OnRowPreviewMouseDown(object sender, MouseButtonEventArgs e) => _dragSource.OnPreviewMouseDown(e);

    private void OnRowPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        _isDragging = true;
        try
        {
            _dragSource.OnPreviewMouseMove(e, element);
        }
        finally
        {
            _isDragging = false;
            if (!IsActive)
            {
                Close(); // soltou fora de um alvo válido ou a barra roubou o foco — fecha como um clique fora faria
            }
        }
    }
}
