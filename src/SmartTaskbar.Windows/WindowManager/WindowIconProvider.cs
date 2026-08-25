using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;

namespace SmartTaskbar.Windows.WindowManager;

/// <summary>
/// Extrai o ícone do executável de uma janela e devolve bytes PNG, prontos para virar
/// <c>BitmapImage</c> na camada de apresentação (que não é referenciada aqui, mantendo
/// este projeto livre de WPF). Cacheia por caminho de executável — extração de ícone é
/// I/O relativamente caro e o mesmo exe se repete entre janelas.
/// </summary>
public sealed class WindowIconProvider
{
    private readonly ConcurrentDictionary<string, byte[]?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public byte[]? GetIconPng(string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return null;
        }

        return _cache.GetOrAdd(executablePath, ExtractIconPng);
    }

    private static byte[]? ExtractIconPng(string executablePath)
    {
        try
        {
            using var icon = Icon.ExtractAssociatedIcon(executablePath);
            if (icon is null)
            {
                return null;
            }

            using var bitmap = icon.ToBitmap();
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            // Executável inacessível, caminho vazio/UWP virtualizado, etc. — segue sem ícone.
            return null;
        }
    }
}
