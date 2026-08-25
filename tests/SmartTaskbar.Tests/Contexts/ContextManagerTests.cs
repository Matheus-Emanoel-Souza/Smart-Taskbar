using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Core.Models;
using SmartTaskbar.Tests.Fakes;

namespace SmartTaskbar.Tests.Contexts;

public class ContextManagerTests
{
    private static ContextManager CreateManager() => new(new InMemoryContextRepository());

    [Fact]
    public void New_Manager_AlwaysHasUncategorizedContext()
    {
        var manager = CreateManager();

        Assert.Contains(manager.All, c => c.Id == Context.UncategorizedId);
    }

    [Fact]
    public void Create_AddsContext_AtEndOfOrder()
    {
        var manager = CreateManager();

        var dev = manager.Create("Desenvolvimento", "🧑‍💻");
        var work = manager.Create("Trabalho", "💼");

        Assert.True(manager.Find(dev.Id)!.Order < manager.Find(work.Id)!.Order);
    }

    [Fact]
    public void Rename_UpdatesName()
    {
        var manager = CreateManager();
        var context = manager.Create("Dev", "🧑‍💻");

        manager.Rename(context.Id, "Desenvolvimento");

        Assert.Equal("Desenvolvimento", manager.Find(context.Id)!.Name);
    }

    [Fact]
    public void Delete_RemovesContext()
    {
        var manager = CreateManager();
        var context = manager.Create("Temporário", "🗑️");

        manager.Delete(context.Id);

        Assert.Null(manager.Find(context.Id));
    }

    [Fact]
    public void Delete_UncategorizedContext_Throws()
    {
        var manager = CreateManager();

        Assert.Throws<InvalidOperationException>(() => manager.Delete(Context.UncategorizedId));
    }

    [Fact]
    public void Reorder_MovesContextToRequestedPosition()
    {
        var manager = CreateManager();
        var a = manager.Create("A", "🅰️");
        var b = manager.Create("B", "🅱️");
        var c = manager.Create("C", "🇨");

        manager.Reorder(c.Id, 0);

        var orderedIds = manager.All.Where(x => x.Id != Context.UncategorizedId).Select(x => x.Id).ToList();
        Assert.Equal([c.Id, a.Id, b.Id], orderedIds);
    }

    [Fact]
    public void Persists_AcrossNewManagerInstance_WithSameRepository()
    {
        var repository = new InMemoryContextRepository();
        var first = new ContextManager(repository);
        first.Create("Internet", "🌐");

        var second = new ContextManager(repository);

        Assert.Contains(second.All, c => c.Name == "Internet");
    }
}
