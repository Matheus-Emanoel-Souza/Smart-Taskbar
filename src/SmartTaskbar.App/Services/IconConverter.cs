using System.IO;
using System.Windows.Media.Imaging;

namespace SmartTaskbar.App.Services;

/// <summary>Converte os bytes PNG entregues por <see cref="Windows.WindowManager.WindowIconProvider"/> em imagem WPF.</summary>
internal static class IconConverter
{
    public static BitmapImage? FromPng(byte[]? pngBytes)
    {
        if (pngBytes is null || pngBytes.Length == 0)
        {
            return null;
        }

        using var stream = new MemoryStream(pngBytes);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze(); // permite uso entre threads e evita realocação a cada binding
        return image;
    }
}
