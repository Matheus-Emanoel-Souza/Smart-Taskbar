using SmartTaskbar.Core.Models;
using SmartTaskbar.Core.Persistence;

namespace SmartTaskbar.Infrastructure.Persistence;

public sealed class JsonWindowAssignmentRepository : IWindowAssignmentRepository
{
    private readonly string _filePath;

    public JsonWindowAssignmentRepository(string dataDirectory)
    {
        _filePath = Path.Combine(dataDirectory, "window-assignments.json");
    }

    public IReadOnlyList<WindowAssignment> Load() => JsonFileStore.Load<WindowAssignment>(_filePath);

    public void Save(IReadOnlyList<WindowAssignment> assignments) => JsonFileStore.Save(_filePath, assignments);
}
