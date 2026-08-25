namespace SmartTaskbar.Core.Models;

/// <summary>
/// Gera a chave estável usada para atribuições manuais de janela a contexto
/// (ver <see cref="WindowInfo.StableKey"/> e <see cref="WindowAssignment"/>).
/// Combina executável + título porque duas janelas do mesmo processo (ex.: abas do
/// Chrome) podem pertencer a contextos diferentes — o requisito central do produto.
/// </summary>
public static class WindowKey
{
    public static string Create(string executablePath, string processName, string title)
    {
        var exePart = string.IsNullOrWhiteSpace(executablePath) ? processName : executablePath;
        return $"{exePart.Trim().ToLowerInvariant()}|{title.Trim().ToLowerInvariant()}";
    }
}
