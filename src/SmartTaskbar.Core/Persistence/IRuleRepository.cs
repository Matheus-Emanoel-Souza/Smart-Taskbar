using SmartTaskbar.Core.Rules;

namespace SmartTaskbar.Core.Persistence;

/// <summary>Abstração de armazenamento de regras. Implementação concreta vive em Infrastructure.</summary>
public interface IRuleRepository
{
    IReadOnlyList<Rule> Load();
    void Save(IReadOnlyList<Rule> rules);
}
