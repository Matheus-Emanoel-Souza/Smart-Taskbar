using SmartTaskbar.Core.Persistence;

namespace SmartTaskbar.Core.Rules;

/// <summary>CRUD de <see cref="Rule"/>. Mantém a lista em memória e persiste a cada mutação.</summary>
public sealed class RuleManager
{
    private readonly IRuleRepository _repository;
    private readonly List<Rule> _rules;

    public RuleManager(IRuleRepository repository)
    {
        _repository = repository;
        _rules = repository.Load().ToList();
    }

    public IReadOnlyList<Rule> All => _rules.OrderBy(r => r.Priority).ToList();

    public Rule Add(Guid contextId, RuleField field, RuleOperator @operator, string value)
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            ContextId = contextId,
            Field = field,
            Operator = @operator,
            Value = value,
            Priority = _rules.Count == 0 ? 0 : _rules.Max(r => r.Priority) + 1,
        };

        _rules.Add(rule);
        Persist();
        return rule;
    }

    public void Remove(Guid id)
    {
        _rules.RemoveAll(r => r.Id == id);
        Persist();
    }

    public void RemoveForContext(Guid contextId)
    {
        _rules.RemoveAll(r => r.ContextId == contextId);
        Persist();
    }

    private void Persist() => _repository.Save(_rules);
}
