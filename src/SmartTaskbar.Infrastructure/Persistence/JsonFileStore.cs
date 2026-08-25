using System.Text.Json;

namespace SmartTaskbar.Infrastructure.Persistence;

/// <summary>
/// Leitura/escrita genérica de uma lista em um arquivo JSON. Escrita é atômica (grava em
/// arquivo temporário e substitui) para não corromper o arquivo se o processo for encerrado
/// no meio da gravação.
/// </summary>
internal static class JsonFileStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public static List<T> Load<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<T>>(json, Options) ?? [];
    }

    public static void Save<T>(string filePath, IReadOnlyList<T> items)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(items, Options);
        var tempPath = $"{filePath}.tmp";
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, filePath, overwrite: true);
    }
}
