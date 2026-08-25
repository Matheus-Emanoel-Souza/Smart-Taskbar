using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SmartTaskbar.App.Services;
using SmartTaskbar.App.ViewModels;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Core.Rules;
using SmartTaskbar.Windows.WindowManager;

namespace SmartTaskbar.App.Views;

/// <summary>
/// Gerenciador em tela cheia: todos os contextos como colunas lado a lado (como pastas num
/// gerenciador de arquivos), para organizar/mover janelas entre eles sem abrir um painel por vez.
/// Janela normal (com barra de título, redimensionável) — não é um popup que fecha sozinho.
/// </summary>
public partial class ManagerWindow : Window
{
    private readonly ManagerViewModel _viewModel;
    private readonly WindowDragSource _dragSource = new();

    public ManagerWindow(
        TaskbarOrchestrator orchestrator,
        ContextManager contexts,
        RuleManager rules,
        WindowAssignmentManager assignments,
        WindowActivator activator,
        WindowIconProvider icons)
    {
        InitializeComponent();

        _viewModel = new ManagerViewModel(orchestrator, contexts, rules, assignments, activator, icons);
        DataContext = _viewModel;

        Closed += (_, _) => _viewModel.Dispose();
    }

    private void OnRowPreviewMouseDown(object sender, MouseButtonEventArgs e) => _dragSource.OnPreviewMouseDown(e);

    private void OnRowPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            _dragSource.OnPreviewMouseMove(e, element);
        }
    }

    private void OnColumnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is Border border && e.Data.GetDataPresent(DragFormats.WindowItem))
        {
            border.Background = (Brush)FindResource("SurfaceHoverBrush");
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void OnColumnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is Border border)
        {
            border.ClearValue(BackgroundProperty);
        }
    }

    private void OnColumnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Border { DataContext: ManagerColumnViewModel column } border)
        {
            return;
        }

        border.ClearValue(BackgroundProperty);

        if (e.Data.GetData(DragFormats.WindowItem) is WindowItemViewModel windowVm)
        {
            windowVm.MoveToContext(column.Header.Id);
        }
    }
}
