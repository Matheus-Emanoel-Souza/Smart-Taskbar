using SmartTaskbar.Core.Models;

namespace SmartTaskbar.Core.Persistence;

/// <summary>Abstração de armazenamento das atribuições manuais janela → contexto.</summary>
public interface IWindowAssignmentRepository
{
    IReadOnlyList<WindowAssignment> Load();
    void Save(IReadOnlyList<WindowAssignment> assignments);
}
