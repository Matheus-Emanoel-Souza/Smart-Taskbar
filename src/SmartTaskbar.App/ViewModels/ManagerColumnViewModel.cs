using System.Collections.ObjectModel;

namespace SmartTaskbar.App.ViewModels;

/// <summary>Uma coluna do gerenciador em tela cheia — um contexto e suas janelas.</summary>
public sealed class ManagerColumnViewModel
{
    public required ContextViewModel Header { get; init; }
    public ObservableCollection<WindowItemViewModel> Windows { get; } = [];
}
