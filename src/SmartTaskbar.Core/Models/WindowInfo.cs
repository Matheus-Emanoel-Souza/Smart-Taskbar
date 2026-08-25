namespace SmartTaskbar.Core.Models;

/// <summary>
/// Snapshot imutável de uma janela top-level do Windows no instante em que foi capturada.
/// Não referencia tipos de UI/Win32 — só dados primitivos, para manter o Core independente
/// de plataforma e testável sem um sistema Windows real.
/// </summary>
public sealed record WindowInfo
{
    /// <summary>Handle nativo da janela (HWND). Válido apenas enquanto a janela existir.</summary>
    public required nint Handle { get; init; }

    public required int ProcessId { get; init; }

    /// <summary>Nome do processo sem extensão (ex.: "chrome").</summary>
    public required string ProcessName { get; init; }

    /// <summary>Caminho completo do executável. Vazio quando não foi possível resolver (processo protegido).</summary>
    public required string ExecutablePath { get; init; }

    public required string Title { get; init; }

    public bool IsMinimized { get; init; }

    /// <summary>
    /// Chave estável usada para vincular esta janela a atribuições manuais de contexto,
    /// já que o handle nativo (<see cref="Handle"/>) some quando a janela é fechada e um
    /// novo handle é criado na próxima abertura.
    /// </summary>
    public string StableKey => WindowKey.Create(ExecutablePath, ProcessName, Title);
}
