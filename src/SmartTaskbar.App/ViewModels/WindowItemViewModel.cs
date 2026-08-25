using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using SmartTaskbar.App.Services;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Core.Models;

namespace SmartTaskbar.App.ViewModels;

/// <summary>Uma janela listada dentro do painel de um contexto.</summary>
public sealed class WindowItemViewModel
{
    private readonly WindowInfo _window;
    private readonly WindowAssignmentManager _assignments;
    private readonly Action _onMoved;

    public string Title => _window.Title;
    public string ProcessName => _window.ProcessName;
    public BitmapImage? Icon { get; }

    public IRelayCommand ActivateCommand { get; }
    public IReadOnlyList<MoveTargetViewModel> MoveTargets { get; }
    public IRelayCommand CreateContextAndMoveCommand { get; }

    public WindowItemViewModel(
        WindowInfo window,
        Guid currentContextId,
        byte[]? iconPng,
        ContextManager contexts,
        WindowAssignmentManager assignments,
        Windows.WindowManager.WindowActivator activator,
        Func<Context?> requestNewContext,
        Action onMoved)
    {
        _window = window;
        _assignments = assignments;
        _onMoved = onMoved;
        Icon = IconConverter.FromPng(iconPng);

        ActivateCommand = new RelayCommand(() => activator.Activate(_window.Handle));

        MoveTargets = contexts.All
            .Where(c => c.Id != currentContextId)
            .Select(c => new MoveTargetViewModel
            {
                Name = c.Name,
                Icon = c.Icon,
                Command = new RelayCommand(() => MoveTo(c.Id)),
            })
            .ToList();

        CreateContextAndMoveCommand = new RelayCommand(() =>
        {
            var created = requestNewContext();
            if (created is not null)
            {
                MoveTo(created.Id);
            }
        });
    }

    /// <summary>Usado pelo drag-and-drop (arrastar a janela até uma pílula de contexto na barra).</summary>
    public void MoveToContext(Guid contextId) => MoveTo(contextId);

    private void MoveTo(Guid contextId)
    {
        _assignments.Assign(_window.StableKey, contextId);
        _onMoved();
    }
}
