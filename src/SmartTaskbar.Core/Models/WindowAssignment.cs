namespace SmartTaskbar.Core.Models;

/// <summary>
/// Atribuição manual de uma janela específica a um contexto (menu "Mover para...").
/// Tem precedência sobre o resultado do <see cref="Rules.RuleEngine"/>.
/// </summary>
public sealed class WindowAssignment
{
    /// <summary>Ver <see cref="WindowInfo.StableKey"/>.</summary>
    public required string WindowKey { get; init; }

    public required Guid ContextId { get; set; }
}
