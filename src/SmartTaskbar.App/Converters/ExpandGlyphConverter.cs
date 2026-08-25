using System.Globalization;
using System.Windows.Data;

namespace SmartTaskbar.App.Converters;

/// <summary>Glifo do botão que minimiza/expande a barra inteira, conforme o estado atual.</summary>
public sealed class ExpandGlyphConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? "–" : "▸";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
