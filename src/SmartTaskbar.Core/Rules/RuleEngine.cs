using SmartTaskbar.Core.Models;

namespace SmartTaskbar.Core.Rules;

/// <summary>Avalia um conjunto de <see cref="Rule"/> contra uma <see cref="WindowInfo"/>.</summary>
public sealed class RuleEngine
{
    /// <summary>
    /// Retorna o Id do contexto da primeira regra que casar, em ordem de <see cref="Rule.Priority"/>
    /// (menor primeiro); <c>null</c> se nenhuma regra casar.
    /// </summary>
    public Guid? Classify(WindowInfo window, IEnumerable<Rule> rules)
    {
        foreach (var rule in rules.OrderBy(r => r.Priority))
        {
            if (Matches(window, rule))
            {
                return rule.ContextId;
            }
        }

        return null;
    }

    public bool Matches(WindowInfo window, Rule rule)
    {
        var fieldValue = rule.Field switch
        {
            RuleField.ProcessName => window.ProcessName,
            RuleField.ExecutablePath => window.ExecutablePath,
            RuleField.WindowTitle => window.Title,
            _ => throw new ArgumentOutOfRangeException(nameof(rule), rule.Field, "Campo de regra desconhecido."),
        };

        return rule.Operator switch
        {
            RuleOperator.Equals => string.Equals(fieldValue, rule.Value, StringComparison.OrdinalIgnoreCase),
            RuleOperator.Contains => fieldValue.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
            RuleOperator.StartsWith => fieldValue.StartsWith(rule.Value, StringComparison.OrdinalIgnoreCase),
            RuleOperator.EndsWith => fieldValue.EndsWith(rule.Value, StringComparison.OrdinalIgnoreCase),
            _ => throw new ArgumentOutOfRangeException(nameof(rule), rule.Operator, "Operador de regra desconhecido."),
        };
    }
}
