namespace SmartTaskbar.Infrastructure.Settings;

/// <summary>
/// Resolve onde o Smart Taskbar grava seus dados: por padrão, uma pasta "Data" ao lado do
/// executável (uso portátil — extrai o ZIP e o app anda com ele). Se a pasta do executável
/// não for gravável (ex.: instalado num diretório protegido em ambiente corporativo),
/// cai silenciosamente para %LOCALAPPDATA%\SmartTaskbar.
/// </summary>
public sealed class PortablePathProvider
{
    private const string DataFolderName = "Data";
    private const string AppFolderName = "SmartTaskbar";

    private readonly Lazy<string> _dataDirectory;

    public PortablePathProvider()
    {
        _dataDirectory = new Lazy<string>(ResolveDataDirectory);
    }

    public string DataDirectory => _dataDirectory.Value;

    private static string ResolveDataDirectory()
    {
        var portableDir = Path.Combine(AppContext.BaseDirectory, DataFolderName);
        if (TryEnsureWritable(portableDir))
        {
            return portableDir;
        }

        var fallbackDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppFolderName);
        Directory.CreateDirectory(fallbackDir);
        return fallbackDir;
    }

    private static bool TryEnsureWritable(string directory)
    {
        try
        {
            Directory.CreateDirectory(directory);
            var probeFile = Path.Combine(directory, $".write-probe-{Guid.NewGuid():N}");
            File.WriteAllText(probeFile, string.Empty);
            File.Delete(probeFile);
            return true;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return false;
        }
    }
}
