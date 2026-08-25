using SmartTaskbar.Windows.Win32;

namespace SmartTaskbar.Windows.WindowEvents;

/// <summary>
/// Observa mudanças de janela via <c>SetWinEventHook</c> em vez de polling: consome ~0% de CPU
/// enquanto ocioso, reage no instante em que o Windows notifica. Roda em thread dedicada com
/// seu próprio loop de mensagens (ver <see cref="NativeMethods.GetMessage"/>), desacoplada do
/// Dispatcher do WPF para não competir com a UI.
/// </summary>
public sealed class WindowEventWatcher : IDisposable
{
    private readonly List<nint> _hookHandles = [];
    private NativeMethods.WinEventDelegate? _callback;
    private Thread? _thread;
    private volatile uint _threadId;
    private readonly ManualResetEventSlim _threadReady = new(false);

    public event EventHandler<WindowChangedEventArgs>? WindowChanged;

    public void Start()
    {
        if (_thread is not null)
        {
            return;
        }

        _thread = new Thread(ThreadMain)
        {
            IsBackground = true,
            Name = "SmartTaskbar.WindowEventWatcher",
        };
        _thread.Start();
        _threadReady.Wait();
    }

    public void Stop()
    {
        if (_thread is null || _threadId == 0)
        {
            return;
        }

        NativeMethods.PostThreadMessage(_threadId, NativeConstants.WM_QUIT, 0, 0);
        _thread.Join(TimeSpan.FromSeconds(2));
        _thread = null;
    }

    private void ThreadMain()
    {
        _threadId = NativeMethods.GetCurrentThreadId();
        _callback = OnWinEvent; // mantém o delegate vivo para o hook nativo não apontar pra memória coletada

        RegisterHook(NativeConstants.EVENT_SYSTEM_FOREGROUND, NativeConstants.EVENT_SYSTEM_FOREGROUND);
        RegisterHook(NativeConstants.EVENT_SYSTEM_MINIMIZESTART, NativeConstants.EVENT_SYSTEM_MINIMIZEEND);
        RegisterHook(NativeConstants.EVENT_OBJECT_CREATE, NativeConstants.EVENT_OBJECT_HIDE);
        RegisterHook(NativeConstants.EVENT_OBJECT_NAMECHANGE, NativeConstants.EVENT_OBJECT_NAMECHANGE);

        _threadReady.Set();

        while (NativeMethods.GetMessage(out var msg, 0, 0, 0) > 0)
        {
            NativeMethods.TranslateMessage(ref msg);
            NativeMethods.DispatchMessage(ref msg);
        }

        foreach (var hook in _hookHandles)
        {
            NativeMethods.UnhookWinEvent(hook);
        }

        _hookHandles.Clear();
    }

    private void RegisterHook(uint eventMin, uint eventMax)
    {
        var hook = NativeMethods.SetWinEventHook(
            eventMin, eventMax, 0, _callback!, idProcess: 0, idThread: 0,
            NativeConstants.WINEVENT_OUTOFCONTEXT | NativeConstants.WINEVENT_SKIPOWNPROCESS);

        if (hook != 0)
        {
            _hookHandles.Add(hook);
        }
    }

    private void OnWinEvent(nint hookHandle, uint eventType, nint hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
    {
        // idObject != OBJID_WINDOW cobre eventos de controles internos (menu, cursor, etc.),
        // que não interessam ao agrupamento por janela top-level.
        if (hwnd == 0 || idObject != NativeConstants.OBJID_WINDOW)
        {
            return;
        }

        WindowChangeKind? kind = eventType switch
        {
            NativeConstants.EVENT_SYSTEM_FOREGROUND => WindowChangeKind.ForegroundChanged,
            NativeConstants.EVENT_OBJECT_CREATE => WindowChangeKind.Created,
            NativeConstants.EVENT_OBJECT_DESTROY => WindowChangeKind.Destroyed,
            NativeConstants.EVENT_OBJECT_SHOW => WindowChangeKind.Shown,
            NativeConstants.EVENT_OBJECT_HIDE => WindowChangeKind.Hidden,
            NativeConstants.EVENT_OBJECT_NAMECHANGE => WindowChangeKind.TitleChanged,
            NativeConstants.EVENT_SYSTEM_MINIMIZESTART or NativeConstants.EVENT_SYSTEM_MINIMIZEEND => WindowChangeKind.MinimizeStateChanged,
            _ => null,
        };

        if (kind is null)
        {
            return;
        }

        WindowChanged?.Invoke(this, new WindowChangedEventArgs { Kind = kind.Value, Handle = hwnd });
    }

    public void Dispose()
    {
        Stop();
        _threadReady.Dispose();
    }
}
