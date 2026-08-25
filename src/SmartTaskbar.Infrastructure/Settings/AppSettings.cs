namespace SmartTaskbar.Infrastructure.Settings;

/// <summary>Preferências gerais persistidas, fora do escopo de contextos/regras.</summary>
public sealed class AppSettings
{
    /// <summary>Posição da barra flutuante. <see cref="double.NaN"/> = ainda não posicionada (usar padrão).</summary>
    public double BarX { get; set; } = double.NaN;

    public double BarY { get; set; } = double.NaN;
}
