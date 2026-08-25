using System.Text;
using SmartTaskbar.Core.Models;
using SmartTaskbar.Windows.ProcessManager;
using SmartTaskbar.Windows.Win32;

namespace SmartTaskbar.Windows.WindowManager;

/// <summary>Captura o snapshot atual de janelas de aplicativo abertas no desktop.</summary>
public sealed class WindowEnumerator
{
    private readonly ProcessInfoProvider _processInfo;

    public WindowEnumerator(ProcessInfoProvider processInfo)
    {
        _processInfo = processInfo;
    }

    public IReadOnlyList<WindowInfo> EnumerateApplicationWindows()
    {
        var result = new List<WindowInfo>();

        NativeMethods.EnumWindows((hWnd, _) =>
        {
            if (WindowFilter.IsApplicationWindow(hWnd))
            {
                var info = TryDescribe(hWnd);
                if (info is not null)
                {
                    result.Add(info);
                }
            }

            return true; // continua enumerando
        }, 0);

        return result;
    }

    private WindowInfo? TryDescribe(nint hWnd)
    {
        var title = GetWindowTitle(hWnd);
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        NativeMethods.GetWindowThreadProcessId(hWnd, out var pid);
        if (pid == 0)
        {
            return null;
        }

        var (processName, executablePath) = _processInfo.Resolve(pid);

        return new WindowInfo
        {
            Handle = hWnd,
            ProcessId = (int)pid,
            ProcessName = processName,
            ExecutablePath = executablePath,
            Title = title,
            IsMinimized = NativeMethods.IsIconic(hWnd),
        };
    }

    private static string GetWindowTitle(nint hWnd)
    {
        var length = NativeMethods.GetWindowTextLengthW(hWnd);
        if (length == 0)
        {
            return string.Empty;
        }

        var buffer = new StringBuilder(length + 1);
        NativeMethods.GetWindowText(hWnd, buffer, buffer.Capacity);
        return buffer.ToString();
    }
}
