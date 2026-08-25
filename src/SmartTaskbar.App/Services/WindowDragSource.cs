using System.Windows;
using System.Windows.Input;
using SmartTaskbar.App.ViewModels;

namespace SmartTaskbar.App.Services;

/// <summary>
/// Lógica de iniciar drag-and-drop a partir de uma linha de janela (arrastar até um contexto
/// para mover). Compartilhada entre <see cref="Views.ContextPanelWindow"/> e
/// <see cref="Views.ManagerWindow"/> para não duplicar o cálculo de limiar de arraste.
/// </summary>
internal sealed class WindowDragSource
{
    private Point? _dragStartPoint;

    public void OnPreviewMouseDown(MouseButtonEventArgs e) => _dragStartPoint = e.GetPosition(null);

    /// <returns>true se um drag foi efetivamente iniciado (bloqueante até soltar ou cancelar).</returns>
    public bool OnPreviewMouseMove(MouseEventArgs e, FrameworkElement element)
    {
        if (_dragStartPoint is null || e.LeftButton != MouseButtonState.Pressed)
        {
            return false;
        }

        var current = e.GetPosition(null);
        var delta = _dragStartPoint.Value - current;
        if (Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return false;
        }

        _dragStartPoint = null;

        if (element.DataContext is not WindowItemViewModel windowVm)
        {
            return false;
        }

        // O Button já capturou o mouse sozinho ao processar seu próprio MouseLeftButtonDown
        // (antes deste Preview terminar de rodar). Com essa captura ainda presa ao Button,
        // DoDragDrop nunca inicia o drag OLE de verdade — é preciso soltar antes.
        element.ReleaseMouseCapture();

        var payload = new DataObject(DragFormats.WindowItem, windowVm);
        DragDrop.DoDragDrop(element, payload, DragDropEffects.Move);
        return true;
    }
}
