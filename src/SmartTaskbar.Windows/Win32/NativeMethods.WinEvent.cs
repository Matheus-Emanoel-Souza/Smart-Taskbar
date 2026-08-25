using System.Runtime.InteropServices;

namespace SmartTaskbar.Windows.Win32;

/// <summary>
/// P/Invoke para <c>SetWinEventHook</c> e o loop de mensagens mínimo necessário para recebê-lo.
/// Não é preciso criar uma janela real: com <c>WINEVENT_OUTOFCONTEXT</c>, o Win32 entrega os
/// callbacks internamente durante <c>GetMessage</c> na thread que registrou o hook — basta essa
/// thread manter um loop de mensagens rodando. Usado por <see cref="WindowEvents.WindowEventWatcher"/>
/// para detectar mudanças de janela por evento nativo em vez de polling.
/// </summary>
internal static partial class NativeMethods
{
    public delegate void WinEventDelegate(
        nint hWinEventHook, uint eventType, nint hwnd,
        int idObject, int idChild, uint idEventThread, uint dwmsEventTime);

    [DllImport("user32.dll")]
    public static extern nint SetWinEventHook(
        uint eventMin, uint eventMax, nint hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWinEvent(nint hWinEventHook);

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public nint hwnd;
        public uint message;
        public nint wParam;
        public nint lParam;
        public uint time;
        public int ptX;
        public int ptY;
    }

    [DllImport("user32.dll")]
    public static extern int GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    public static extern nint DispatchMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostThreadMessage(uint idThread, uint msg, nint wParam, nint lParam);
}
