namespace SmartTaskbar.Core.Models;

/// <summary>Um agrupamento de janelas por contexto de uso (Trabalho, Desenvolvimento, ...).</summary>
public sealed class Context
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }

    /// <summary>Ícone do contexto, hoje um emoji (ex.: "🧑‍💻"). Trocar por asset próprio é compatível no futuro.</summary>
    public required string Icon { get; set; }

    /// <summary>Posição de exibição na barra; menor aparece primeiro.</summary>
    public int Order { get; set; }

    /// <summary>Id reservado do contexto padrão "Sem contexto", criado automaticamente e não removível.</summary>
    public static readonly Guid UncategorizedId = new("00000000-0000-0000-0000-000000000001");

    public static Context CreateUncategorized() => new()
    {
        Id = UncategorizedId,
        Name = "Sem contexto",
        Icon = "🗂️",
        Order = int.MaxValue,
    };
}
