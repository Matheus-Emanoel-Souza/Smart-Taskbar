using SmartTaskbar.Core.Models;
using SmartTaskbar.Core.Persistence;

namespace SmartTaskbar.Core.Contexts;

/// <summary>
/// Atribuições manuais de janela → contexto (menu "Mover para..."). Têm precedência sobre
/// o <see cref="Rules.RuleEngine"/> — ver <see cref="Classification.WindowClassifier"/>.
/// </summary>
public sealed class WindowAssignmentManager
{
    private readonly IWindowAssignmentRepository _repository;
    private readonly Dictionary<string, WindowAssignment> _byKey;

    public WindowAssignmentManager(IWindowAssignmentRepository repository)
    {
        _repository = repository;
        _byKey = repository.Load().ToDictionary(a => a.WindowKey);
    }

    public Guid? FindContext(string windowKey) =>
        _byKey.TryGetValue(windowKey, out var assignment) ? assignment.ContextId : null;

    public void Assign(string windowKey, Guid contextId)
    {
        _byKey[windowKey] = new WindowAssignment { WindowKey = windowKey, ContextId = contextId };
        Persist();
    }

    public void Unassign(string windowKey)
    {
        if (_byKey.Remove(windowKey))
        {
            Persist();
        }
    }

    /// <summary>
    /// Redireciona todas as atribuições que apontam para <paramref name="fromContextId"/> para
    /// <paramref name="toContextId"/>. Usado ao excluir um contexto, para não deixar janelas
    /// presas a um Id que não existe mais.
    /// </summary>
    public void ReassignContext(Guid fromContextId, Guid toContextId)
    {
        var affected = _byKey.Values.Where(a => a.ContextId == fromContextId).ToList();
        if (affected.Count == 0)
        {
            return;
        }

        foreach (var assignment in affected)
        {
            assignment.ContextId = toContextId;
        }

        Persist();
    }

    private void Persist() => _repository.Save(_byKey.Values.ToList());
}
