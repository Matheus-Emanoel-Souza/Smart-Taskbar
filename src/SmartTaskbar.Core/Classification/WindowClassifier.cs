using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Core.Models;
using SmartTaskbar.Core.Rules;

namespace SmartTaskbar.Core.Classification;

/// <summary>
/// Decide a que contexto uma janela pertence, combinando:
/// 1. atribuição manual do usuário ("Mover para...") — sempre vence;
/// 2. primeira regra que casar, por prioridade;
/// 3. contexto "Sem contexto" como fallback.
/// </summary>
public sealed class WindowClassifier
{
    private readonly WindowAssignmentManager _assignments;
    private readonly RuleManager _rules;
    private readonly RuleEngine _engine;

    public WindowClassifier(WindowAssignmentManager assignments, RuleManager rules, RuleEngine engine)
    {
        _assignments = assignments;
        _rules = rules;
        _engine = engine;
    }

    public Guid Classify(WindowInfo window)
    {
        var manual = _assignments.FindContext(window.StableKey);
        if (manual is { } manualContextId)
        {
            return manualContextId;
        }

        var byRule = _engine.Classify(window, _rules.All);
        return byRule ?? Context.UncategorizedId;
    }
}
