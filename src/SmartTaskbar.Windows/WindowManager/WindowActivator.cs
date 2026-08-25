using SmartTaskbar.Windows.Win32;

namespace SmartTaskbar.Windows.WindowManager;

/// <summary>Restaura e traz uma janela para primeiro plano, ao clicar em um item no painel de contexto.</summary>
public sealed class WindowActivator
{
    public void Activate(nint hWnd)
    {
        if (!NativeMethods.IsWindow(hWnd))
        {
            return;
        }

        if (NativeMethods.IsIconic(hWnd))
        {
            NativeMethods.ShowWindow(hWnd, NativeConstants.SW_RESTORE);
        }
        else
        {
            NativeMethods.ShowWindow(hWnd, NativeConstants.SW_SHOW);
        }

        if (NativeMethods.SetForegroundWindow(hWnd))
        {
            return;
        }

        // O Windows recusa SetForegroundWindow vindo de processo "em background" (foreground
        // lock). Contornar anexando temporariamente a fila de entrada da thread da janela alvo.
        var targetThreadId = NativeMethods.GetWindowThreadProcessId(hWnd, out _);
        var currentThreadId = NativeMethods.GetCurrentThreadId();

        if (targetThreadId == currentThreadId)
        {
            return;
        }

        if (NativeMethods.AttachThreadInput(currentThreadId, targetThreadId, true))
        {
            try
            {
                NativeMethods.SetForegroundWindow(hWnd);
            }
            finally
            {
                NativeMethods.AttachThreadInput(currentThreadId, targetThreadId, false);
            }
        }
    }
}
