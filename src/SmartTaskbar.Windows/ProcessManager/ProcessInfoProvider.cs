using System.Text;
using SmartTaskbar.Windows.Win32;

namespace SmartTaskbar.Windows.ProcessManager;

/// <summary>Resolve caminho do executável e nome de processo a partir de um PID, sem exigir administrador.</summary>
public sealed class ProcessInfoProvider
{
    /// <summary>
    /// Retorna (nome, caminho completo). Caminho vem vazio quando o processo é protegido
    /// (ex.: processos de sistema) e o acesso é negado mesmo com PROCESS_QUERY_LIMITED_INFORMATION —
    /// nesse caso o nome ainda é derivado do PID via <see cref="System.Diagnostics.Process"/>.
    /// </summary>
    public (string ProcessName, string ExecutablePath) Resolve(uint processId)
    {
        var path = TryGetExecutablePath(processId);

        string name;
        if (!string.IsNullOrEmpty(path))
        {
            name = Path.GetFileNameWithoutExtension(path);
        }
        else
        {
            name = TryGetProcessNameFallback(processId);
        }

        return (name, path ?? string.Empty);
    }

    private static string? TryGetExecutablePath(uint processId)
    {
        var handle = NativeMethods.OpenProcess(NativeConstants.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
        if (handle == 0)
        {
            return null;
        }

        try
        {
            var buffer = new StringBuilder(1024);
            var size = (uint)buffer.Capacity;
            return NativeMethods.QueryFullProcessImageName(handle, 0, buffer, ref size)
                ? buffer.ToString(0, (int)size)
                : null;
        }
        finally
        {
            NativeMethods.CloseHandle(handle);
        }
    }

    private static string TryGetProcessNameFallback(uint processId)
    {
        try
        {
            using var process = System.Diagnostics.Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch
        {
            return $"pid-{processId}";
        }
    }
}
