using SmartTaskbar.Core.Models;
using SmartTaskbar.Core.Rules;
using SmartTaskbar.Infrastructure.Persistence;

namespace SmartTaskbar.Tests.Persistence;

public class JsonRepositoryTests : IDisposable
{
    private readonly string _tempDirectory;

    public JsonRepositoryTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SmartTaskbarTests_" + Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void JsonContextRepository_RoundTrips_Contexts()
    {
        var repository = new JsonContextRepository(_tempDirectory);
        var contexts = new List<Context>
        {
            new() { Id = Guid.NewGuid(), Name = "Desenvolvimento", Icon = "🧑‍💻", Order = 0 },
            new() { Id = Guid.NewGuid(), Name = "Trabalho", Icon = "💼", Order = 1 },
        };

        repository.Save(contexts);
        var loaded = repository.Load();

        Assert.Equal(2, loaded.Count);
        Assert.Equal(contexts[0].Name, loaded[0].Name);
        Assert.Equal(contexts[1].Icon, loaded[1].Icon);
    }

    [Fact]
    public void JsonRuleRepository_RoundTrips_Rules()
    {
        var repository = new JsonRuleRepository(_tempDirectory);
        var rules = new List<Rule>
        {
            new() { Id = Guid.NewGuid(), ContextId = Guid.NewGuid(), Field = RuleField.WindowTitle, Operator = RuleOperator.Contains, Value = "localhost", Priority = 0 },
        };

        repository.Save(rules);
        var loaded = repository.Load();

        Assert.Single(loaded);
        Assert.Equal(RuleField.WindowTitle, loaded[0].Field);
        Assert.Equal("localhost", loaded[0].Value);
    }

    [Fact]
    public void Load_WhenFileDoesNotExist_ReturnsEmptyList()
    {
        var repository = new JsonContextRepository(_tempDirectory);

        Assert.Empty(repository.Load());
    }

    [Fact]
    public void Save_CreatesDataDirectory_WhenMissing()
    {
        Assert.False(Directory.Exists(_tempDirectory));

        var repository = new JsonContextRepository(_tempDirectory);
        repository.Save([new Context { Id = Guid.NewGuid(), Name = "X", Icon = "❓", Order = 0 }]);

        Assert.True(Directory.Exists(_tempDirectory));
    }
}
