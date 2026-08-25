namespace SmartTaskbar.Core.Rules;

/// <summary>
/// Regra simples "SE campo OPERADOR valor ENTÃO contexto". Separada da UI para permitir
/// evolução futura (regras compostas, múltiplas condições — ver roadmap V0.3).
/// </summary>
public sealed class Rule
{
    public required Guid Id { get; init; }
    public required Guid ContextId { get; set; }
    public required RuleField Field { get; set; }
    public required RuleOperator Operator { get; set; }
    public required string Value { get; set; }

    /// <summary>Menor valor é avaliado primeiro; a primeira regra que casar decide o contexto.</summary>
    public int Priority { get; set; }
}
