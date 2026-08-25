using System.Windows;

namespace SmartTaskbar.App.Views;

/// <summary>Diálogo modal simples para criar/editar nome e ícone de um contexto.</summary>
public partial class ContextEditWindow : Window
{
    public string ResultName { get; private set; } = string.Empty;
    public string ResultIcon { get; private set; } = string.Empty;

    public ContextEditWindow(string title, string initialName, string initialIcon)
    {
        InitializeComponent();
        Title = title;
        NameBox.Text = initialName;
        IconBox.Text = initialIcon;
        NameBox.Focus();
        NameBox.SelectAll();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            NameBox.Focus();
            return;
        }

        ResultName = name;
        ResultIcon = string.IsNullOrWhiteSpace(IconBox.Text) ? "🗂️" : IconBox.Text.Trim();
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    /// <summary>Mostra o diálogo e retorna (nome, ícone) se confirmado, ou <c>null</c> se cancelado.</summary>
    public static (string Name, string Icon)? Prompt(string title, string initialName, string initialIcon)
    {
        var window = new ContextEditWindow(title, initialName, initialIcon)
        {
            Owner = Application.Current.MainWindow,
        };

        return window.ShowDialog() == true ? (window.ResultName, window.ResultIcon) : null;
    }
}
