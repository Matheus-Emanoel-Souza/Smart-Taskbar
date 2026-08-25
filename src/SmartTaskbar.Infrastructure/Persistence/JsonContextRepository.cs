using SmartTaskbar.Core.Models;
using SmartTaskbar.Core.Persistence;

namespace SmartTaskbar.Infrastructure.Persistence;

public sealed class JsonContextRepository : IContextRepository
{
    private readonly string _filePath;

    public JsonContextRepository(string dataDirectory)
    {
        _filePath = Path.Combine(dataDirectory, "contexts.json");
    }

    public IReadOnlyList<Context> Load() => JsonFileStore.Load<Context>(_filePath);

    public void Save(IReadOnlyList<Context> contexts) => JsonFileStore.Save(_filePath, contexts);
}
