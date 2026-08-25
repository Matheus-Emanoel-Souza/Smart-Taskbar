using SmartTaskbar.Core.Models;
using SmartTaskbar.Core.Persistence;
using SmartTaskbar.Core.Rules;

namespace SmartTaskbar.Tests.Fakes;

/// <summary>Repositórios em memória para testar Core sem tocar em disco.</summary>
public sealed class InMemoryContextRepository : IContextRepository
{
    private List<Context> _contexts = [];

    public IReadOnlyList<Context> Load() => _contexts;

    public void Save(IReadOnlyList<Context> contexts) => _contexts = contexts.ToList();
}

public sealed class InMemoryRuleRepository : IRuleRepository
{
    private List<Rule> _rules = [];

    public IReadOnlyList<Rule> Load() => _rules;

    public void Save(IReadOnlyList<Rule> rules) => _rules = rules.ToList();
}

public sealed class InMemoryWindowAssignmentRepository : IWindowAssignmentRepository
{
    private List<WindowAssignment> _assignments = [];

    public IReadOnlyList<WindowAssignment> Load() => _assignments;

    public void Save(IReadOnlyList<WindowAssignment> assignments) => _assignments = assignments.ToList();
}
