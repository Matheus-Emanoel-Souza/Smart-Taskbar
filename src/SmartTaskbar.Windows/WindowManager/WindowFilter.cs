using SmartTaskbar.Windows.Win32;

namespace SmartTaskbar.Windows.WindowManager;

/// <summary>
/// Decide se um HWND é uma janela "de aplicativo" que deve aparecer nos contextos —
/// mesma heurística usada por reimplementações de taskbar: visível, com título, top-level
/// (sem owner), não ferramenta, não cloaked pelo DWM (UWP suspensa / outra área virtual).
/// </summary>
internal static class WindowFilter
{
    public static bool IsApplicationWindow(nint hWnd)
    {
        if (!NativeMethods.IsWindowVisible(hWnd))
        {
            return false;
        }

        if (NativeMethods.GetWindowTextLengthW(hWnd) == 0)
        {
            return false;
        }

        // Janela com owner normalmente é um popup/diálogo, não uma janela "principal".
        if (NativeMethods.GetWindow(hWnd, NativeConstants.GW_OWNER) != 0)
        {
            return false;
        }

        var exStyle = (long)NativeMethods.GetWindowLongPtr(hWnd, NativeConstants.GWL_EXSTYLE);
        var isToolWindow = (exStyle & NativeConstants.WS_EX_TOOLWINDOW) != 0;
        var isAppWindow = (exStyle & NativeConstants.WS_EX_APPWINDOW) != 0;
        if (isToolWindow && !isAppWindow)
        {
            return false;
        }

        if (IsCloaked(hWnd))
        {
            return false;
        }

        return true;
    }

    private static bool IsCloaked(nint hWnd)
    {
        var hr = NativeMethods.DwmGetWindowAttribute(hWnd, NativeMethods.DWMWA_CLOAKED, out var cloaked, sizeof(int));
        return hr == 0 && cloaked != 0;
    }
}
