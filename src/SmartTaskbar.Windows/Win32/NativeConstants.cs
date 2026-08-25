namespace SmartTaskbar.Windows.Win32;

/// <summary>Constantes Win32 usadas pela camada de integração. Nomes mantidos como na API nativa.</summary>
internal static class NativeConstants
{
    // GetWindowLong
    public const int GWL_EXSTYLE = -20;
    public const int GWL_STYLE = -16;

    public const long WS_EX_TOOLWINDOW = 0x00000080L;
    public const long WS_EX_APPWINDOW = 0x00040000L;
    public const long WS_DISABLED = 0x08000000L;

    // GetWindow
    public const uint GW_OWNER = 4;

    // ShowWindow
    public const int SW_RESTORE = 9;
    public const int SW_SHOW = 5;
    public const int SW_SHOWNOACTIVATE = 4;

    // SetWinEventHook
    public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
    public const uint EVENT_OBJECT_CREATE = 0x8000;
    public const uint EVENT_OBJECT_DESTROY = 0x8001;
    public const uint EVENT_OBJECT_SHOW = 0x8002;
    public const uint EVENT_OBJECT_HIDE = 0x8003;
    public const uint EVENT_OBJECT_NAMECHANGE = 0x800C;
    public const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;
    public const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;

    public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
    public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;

    public const long OBJID_WINDOW = 0;

    // Mensagens de janela / loop de mensagens
    public const uint WM_QUIT = 0x0012;

    // Processo
    public const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
}
