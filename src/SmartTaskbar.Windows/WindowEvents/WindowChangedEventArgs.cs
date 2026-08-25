namespace SmartTaskbar.Windows.WindowEvents;

public sealed class WindowChangedEventArgs : EventArgs
{
    public required WindowChangeKind Kind { get; init; }
    public required nint Handle { get; init; }
}
