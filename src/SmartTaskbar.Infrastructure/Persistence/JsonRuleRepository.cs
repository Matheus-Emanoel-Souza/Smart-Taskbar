using SmartTaskbar.Core.Persistence;
using SmartTaskbar.Core.Rules;

namespace SmartTaskbar.Infrastructure.Persistence;

public sealed class JsonRuleRepository : IRuleRepository
{
    private readonly string _filePath;

    public JsonRuleRepository(string dataDirectory)
    {
        _filePath = Path.Combine(dataDirectory, "rules.json");
    }

    public IReadOnlyList<Rule> Load() => JsonFileStore.Load<Rule>(_filePath);

    public void Save(IReadOnlyList<Rule> rules) => JsonFileStore.Save(_filePath, rules);
}
