using System.Runtime.InteropServices;

namespace SmartTaskbar.Windows.Win32;

internal static partial class NativeMethods
{
    public const uint DWMWA_CLOAKED = 14;

    /// <summary>
    /// Consulta atributos DWM da janela. Usado para detectar janelas "cloaked" (UWP suspensa,
    /// janela em outra área de trabalho virtual) que <c>IsWindowVisible</c> não distingue.
    /// </summary>
    [LibraryImport("dwmapi.dll")]
    public static partial int DwmGetWindowAttribute(nint hwnd, uint dwAttribute, out int pvAttribute, int cbAttribute);
}
