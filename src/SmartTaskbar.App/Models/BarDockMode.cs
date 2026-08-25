namespace SmartTaskbar.App.Models;

/// <summary>Modo de fixação da barra flutuante, escolhido pelo menu de botão direito.</summary>
public enum BarDockMode
{
    /// <summary>Posição livre, arrastável — usa a última posição salva (BarX/BarY).</summary>
    Free,
    Top,
    Bottom,
    Left,
    Right,
}
