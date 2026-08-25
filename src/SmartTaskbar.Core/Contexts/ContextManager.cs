using SmartTaskbar.Core.Models;
using SmartTaskbar.Core.Persistence;

namespace SmartTaskbar.Core.Contexts;

/// <summary>
/// CRUD e reordenação de <see cref="Context"/>. Mantém a lista em memória e persiste a cada
/// mutação (write-through) — volume de escrita é baixo, uso interativo do usuário.
/// </summary>
public sealed class ContextManager
{
    private readonly IContextRepository _repository;
    private readonly List<Context> _contexts;

    public ContextManager(IContextRepository repository)
    {
        _repository = repository;
        _contexts = repository.Load().ToList();

        if (_contexts.All(c => c.Id != Context.UncategorizedId))
        {
            _contexts.Add(Context.CreateUncategorized());
        }
    }

    public IReadOnlyList<Context> All => _contexts.OrderBy(c => c.Order).ToList();

    public Context? Find(Guid id) => _contexts.FirstOrDefault(c => c.Id == id);

    public Context Create(string name, string icon)
    {
        // Exclui "Sem contexto" do cálculo: seu Order é int.MaxValue (sentinela para ficar
        // sempre por último) e entraria no Max()+1, estourando para negativo.
        var nextOrder = _contexts
            .Where(c => c.Id != Context.UncategorizedId)
            .Select(c => c.Order)
            .DefaultIfEmpty(-1)
            .Max() + 1;

        var context = new Context
        {
            Id = Guid.NewGuid(),
            Name = name,
            Icon = icon,
            Order = nextOrder,
        };

        _contexts.Add(context);
        Persist();
        return context;
    }

    public void Rename(Guid id, string newName)
    {
        var context = RequireContext(id);
        context.Name = newName;
        Persist();
    }

    public void ChangeIcon(Guid id, string newIcon)
    {
        var context = RequireContext(id);
        context.Icon = newIcon;
        Persist();
    }

    public void Delete(Guid id)
    {
        if (id == Context.UncategorizedId)
        {
            throw new InvalidOperationException("O contexto padrão \"Sem contexto\" não pode ser excluído.");
        }

        var context = RequireContext(id);
        _contexts.Remove(context);
        Persist();
    }

    /// <summary>Reordena colocando o contexto <paramref name="id"/> na posição <paramref name="newIndex"/> (0-based, entre os já ordenados).</summary>
    public void Reorder(Guid id, int newIndex)
    {
        if (id == Context.UncategorizedId)
        {
            return; // "Sem contexto" não é reordenável, sempre fica por último.
        }

        // Exclui "Sem contexto" da renumeração: seu Order (int.MaxValue) precisa continuar
        // maior que o de todos os outros para ficar sempre por último em All.
        var ordered = _contexts.Where(c => c.Id != Context.UncategorizedId).OrderBy(c => c.Order).ToList();
        var context = ordered.FirstOrDefault(c => c.Id == id)
            ?? throw new InvalidOperationException($"Contexto '{id}' não encontrado.");

        ordered.Remove(context);
        newIndex = Math.Clamp(newIndex, 0, ordered.Count);
        ordered.Insert(newIndex, context);

        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].Order = i;
        }

        Persist();
    }

    private Context RequireContext(Guid id) =>
        Find(id) ?? throw new InvalidOperationException($"Contexto '{id}' não encontrado.");

    private void Persist() => _repository.Save(_contexts);
}
