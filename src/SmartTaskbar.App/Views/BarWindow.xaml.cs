using System.Windows;
using System.Windows.Input;
using SmartTaskbar.App.ViewModels;
using SmartTaskbar.App.Services;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Infrastructure.Settings;
using SmartTaskbar.Windows.WindowManager;

namespace SmartTaskbar.App.Views;

/// <summary>
/// Barra flutuante independente — não substitui a barra de tarefas nativa. Arrastável pela
/// área vazia; posição é salva ao soltar o mouse e restaurada no próximo início.
/// </summary>
public partial class BarWindow : Window
{
    private readonly BarViewModel _viewModel;
    private readonly SettingsService _settings;
    private readonly TaskbarOrchestrator _orchestrator;
    private readonly ContextManager _contexts;
    private readonly WindowAssignmentManager _assignments;
    private readonly WindowActivator _activator;
    private readonly WindowIconProvider _icons;

    private Guid? _openPanelContextId;
    private ContextPanelWindow? _openPanel;

    public BarWindow(
        BarViewModel viewModel,
        SettingsService settings,
        TaskbarOrchestrator orchestrator,
        ContextManager contexts,
        WindowAssignmentManager assignments,
        WindowActivator activator,
        WindowIconProvider icons)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _settings = settings;
        _orchestrator = orchestrator;
        _contexts = contexts;
        _assignments = assignments;
        _activator = activator;
        _icons = icons;

        DataContext = _viewModel;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var saved = _settings.Load();
        if (!double.IsNaN(saved.BarX) && !double.IsNaN(saved.BarY))
        {
            Left = saved.BarX;
            Top = saved.BarY;
            return;
        }

        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left + (workArea.Width - ActualWidth) / 2;
        Top = workArea.Bottom - ActualHeight - 16;
    }

    private void OnDragHandle(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        _openPanel?.Close();
        DragMove();

        var settings = _settings.Load();
        settings.BarX = Left;
        settings.BarY = Top;
        _settings.Save(settings);
    }

    private void OnContextPillClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: ContextViewModel contextVm } button)
        {
            return;
        }

        if (_openPanel is not null)
        {
            var wasSameContext = _openPanelContextId == contextVm.Id;
            _openPanel.Close();
            _openPanel = null;
            _openPanelContextId = null;
            if (wasSameContext)
            {
                return; // clique na mesma pílula fecha o painel (toggle)
            }
        }

        var panel = new ContextPanelWindow(contextVm.Id, _orchestrator, _contexts, _assignments, _activator, _icons)
        {
            Owner = this,
        };

        // PointToScreen devolve pixels físicos; converte para DIPs (o que Window.Left/Top espera)
        // usando a transform do próprio monitor, para posicionar certo também em telas com DPI != 100%.
        var devicePoint = button.PointToScreen(new Point(0, 0));
        var toDip = PresentationSource.FromVisual(button)?.CompositionTarget?.TransformFromDevice;
        var anchor = toDip?.Transform(devicePoint) ?? devicePoint;

        panel.Loaded += (_, _) =>
        {
            panel.Left = anchor.X;
            panel.Top = anchor.Y - panel.ActualHeight - 8;
        };
        panel.Closed += (_, _) =>
        {
            if (ReferenceEquals(_openPanel, panel))
            {
                _openPanel = null;
                _openPanelContextId = null;
            }
        };

        _openPanel = panel;
        _openPanelContextId = contextVm.Id;
        panel.Show();
    }
}
