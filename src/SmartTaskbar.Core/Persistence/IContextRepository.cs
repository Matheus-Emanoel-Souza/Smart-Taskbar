using SmartTaskbar.Core.Models;

namespace SmartTaskbar.Core.Persistence;

/// <summary>Abstração de armazenamento de contextos. Implementação concreta vive em Infrastructure.</summary>
public interface IContextRepository
{
    IReadOnlyList<Context> Load();
    void Save(IReadOnlyList<Context> contexts);
}
