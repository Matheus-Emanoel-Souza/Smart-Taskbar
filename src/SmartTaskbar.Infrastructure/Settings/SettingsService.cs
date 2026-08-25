using System.Text.Json;

namespace SmartTaskbar.Infrastructure.Settings;

/// <summary>Carrega/grava <see cref="AppSettings"/> como um único arquivo JSON.</summary>
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _filePath;
    private AppSettings? _cached;

    public SettingsService(string dataDirectory)
    {
        _filePath = Path.Combine(dataDirectory, "settings.json");
    }

    public AppSettings Load()
    {
        if (_cached is not null)
        {
            return _cached;
        }

        if (!File.Exists(_filePath))
        {
            return _cached = new AppSettings();
        }

        var json = File.ReadAllText(_filePath);
        return _cached = JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        _cached = settings;

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings, Options);
        var tempPath = $"{_filePath}.tmp";
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, _filePath, overwrite: true);
    }
}
