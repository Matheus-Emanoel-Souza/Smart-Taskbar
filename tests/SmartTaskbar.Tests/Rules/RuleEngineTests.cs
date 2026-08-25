using SmartTaskbar.Core.Models;
using SmartTaskbar.Core.Rules;

namespace SmartTaskbar.Tests.Rules;

public class RuleEngineTests
{
    private static WindowInfo MakeWindow(string processName = "chrome", string exePath = @"C:\Chrome\chrome.exe", string title = "") =>
        new()
        {
            Handle = 1,
            ProcessId = 100,
            ProcessName = processName,
            ExecutablePath = exePath,
            Title = title,
        };

    [Theory]
    [InlineData("chrome", RuleOperator.Equals, "chrome", true)]
    [InlineData("chrome", RuleOperator.Equals, "CHROME", true)] // ignore case
    [InlineData("chrome", RuleOperator.Equals, "firefox", false)]
    public void Matches_ProcessNameEquals(string processName, RuleOperator op, string value, bool expected)
    {
        var engine = new RuleEngine();
        var window = MakeWindow(processName: processName);
        var rule = new Rule { Id = Guid.NewGuid(), ContextId = Guid.NewGuid(), Field = RuleField.ProcessName, Operator = op, Value = value };

        Assert.Equal(expected, engine.Matches(window, rule));
    }

    [Fact]
    public void Matches_WindowTitleContains_IsCaseInsensitive()
    {
        var engine = new RuleEngine();
        var window = MakeWindow(title: "Meu Projeto - localhost:5000 - Chrome");
        var rule = new Rule { Id = Guid.NewGuid(), ContextId = Guid.NewGuid(), Field = RuleField.WindowTitle, Operator = RuleOperator.Contains, Value = "LOCALHOST" };

        Assert.True(engine.Matches(window, rule));
    }

    [Fact]
    public void Classify_ReturnsFirstMatchByPriority()
    {
        var engine = new RuleEngine();
        var devContext = Guid.NewGuid();
        var internetContext = Guid.NewGuid();

        var rules = new[]
        {
            new Rule { Id = Guid.NewGuid(), ContextId = internetContext, Field = RuleField.ProcessName, Operator = RuleOperator.Equals, Value = "chrome", Priority = 10 },
            new Rule { Id = Guid.NewGuid(), ContextId = devContext, Field = RuleField.WindowTitle, Operator = RuleOperator.Contains, Value = "localhost", Priority = 0 },
        };

        var window = MakeWindow(processName: "chrome", title: "app - localhost:3000");

        Assert.Equal(devContext, engine.Classify(window, rules));
    }

    [Fact]
    public void Classify_ReturnsNull_WhenNoRuleMatches()
    {
        var engine = new RuleEngine();
        var window = MakeWindow(processName: "notepad", title: "sem-titulo.txt");
        var rules = new[]
        {
            new Rule { Id = Guid.NewGuid(), ContextId = Guid.NewGuid(), Field = RuleField.ProcessName, Operator = RuleOperator.Equals, Value = "chrome" },
        };

        Assert.Null(engine.Classify(window, rules));
    }

    [Fact]
    public void Matches_ExecutablePathStartsWith()
    {
        var engine = new RuleEngine();
        var window = MakeWindow(exePath: @"C:\Program Files\Microsoft VS Code\Code.exe");
        var rule = new Rule { Id = Guid.NewGuid(), ContextId = Guid.NewGuid(), Field = RuleField.ExecutablePath, Operator = RuleOperator.EndsWith, Value = "Code.exe" };

        Assert.True(engine.Matches(window, rule));
    }
}
