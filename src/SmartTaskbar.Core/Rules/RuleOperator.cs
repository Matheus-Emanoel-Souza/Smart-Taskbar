namespace SmartTaskbar.Core.Rules;

/// <summary>Comparação usada por uma <see cref="Rule"/>. Sempre ignora maiúsculas/minúsculas.</summary>
public enum RuleOperator
{
    Equals,
    Contains,
    StartsWith,
    EndsWith,
}
