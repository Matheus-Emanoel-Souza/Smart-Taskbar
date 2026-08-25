using SmartTaskbar.Core.Classification;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Core.Models;
using SmartTaskbar.Core.Rules;
using SmartTaskbar.Tests.Fakes;

namespace SmartTaskbar.Tests.Classification;

public class WindowClassifierTests
{
    private static WindowInfo MakeWindow(string processName, string title, string exePath = "") =>
        new()
        {
            Handle = 1,
            ProcessId = 100,
            ProcessName = processName,
            ExecutablePath = exePath,
            Title = title,
        };

    [Fact]
    public void SameProcess_DifferentTitles_CanGoToDifferentContexts()
    {
        var contexts = new ContextManager(new InMemoryContextRepository());
        var rules = new RuleManager(new InMemoryRuleRepository());
        var assignments = new WindowAssignmentManager(new InMemoryWindowAssignmentRepository());
        var classifier = new WindowClassifier(assignments, rules, new RuleEngine());

        var dev = contexts.Create("Desenvolvimento", "🧑‍💻");
        var internet = contexts.Create("Internet", "🌐");

        rules.Add(dev.Id, RuleField.WindowTitle, RuleOperator.Contains, "localhost");
        rules.Add(internet.Id, RuleField.WindowTitle, RuleOperator.Contains, "YouTube");

        var localhostTab = MakeWindow("chrome", "App - localhost:5000");
        var youtubeTab = MakeWindow("chrome", "Vídeo incrível - YouTube");

        Assert.Equal(dev.Id, classifier.Classify(localhostTab));
        Assert.Equal(internet.Id, classifier.Classify(youtubeTab));
    }

    [Fact]
    public void ManualAssignment_TakesPrecedenceOverRules()
    {
        var contexts = new ContextManager(new InMemoryContextRepository());
        var rules = new RuleManager(new InMemoryRuleRepository());
        var assignments = new WindowAssignmentManager(new InMemoryWindowAssignmentRepository());
        var classifier = new WindowClassifier(assignments, rules, new RuleEngine());

        var internet = contexts.Create("Internet", "🌐");
        var work = contexts.Create("Trabalho", "💼");
        rules.Add(internet.Id, RuleField.ProcessName, RuleOperator.Equals, "chrome");

        var window = MakeWindow("chrome", "Gmail");
        assignments.Assign(window.StableKey, work.Id);

        Assert.Equal(work.Id, classifier.Classify(window));
    }

    [Fact]
    public void NoRuleAndNoAssignment_FallsBackToUncategorized()
    {
        var contexts = new ContextManager(new InMemoryContextRepository());
        var rules = new RuleManager(new InMemoryRuleRepository());
        var assignments = new WindowAssignmentManager(new InMemoryWindowAssignmentRepository());
        var classifier = new WindowClassifier(assignments, rules, new RuleEngine());

        var window = MakeWindow("notepad", "sem-titulo.txt");

        Assert.Equal(Context.UncategorizedId, classifier.Classify(window));
    }
}
