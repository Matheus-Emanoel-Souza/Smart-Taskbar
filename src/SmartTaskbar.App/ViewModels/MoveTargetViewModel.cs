using CommunityToolkit.Mvvm.Input;

namespace SmartTaskbar.App.ViewModels;

/// <summary>Uma opção do submenu "Mover para..." de uma janela.</summary>
public sealed class MoveTargetViewModel
{
    public required string Name { get; init; }
    public required string Icon { get; init; }
    public required IRelayCommand Command { get; init; }
}
