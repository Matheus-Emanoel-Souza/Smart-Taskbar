namespace SmartTaskbar.Windows.WindowEvents;

/// <summary>Tipo de mudança detectada pelo hook nativo de eventos de janela.</summary>
public enum WindowChangeKind
{
    ForegroundChanged,
    Created,
    Destroyed,
    Shown,
    Hidden,
    TitleChanged,
    MinimizeStateChanged,
}
