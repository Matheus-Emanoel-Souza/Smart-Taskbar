using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartTaskbar.App.Views;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Core.Rules;
using Context = SmartTaskbar.Core.Models.Context;

namespace SmartTaskbar.App.ViewModels;

/// <summary>Representa um contexto na barra (pílula) e suas ações de edição via menu de contexto.</summary>
public sealed partial class ContextViewModel : ObservableObject
{
    private readonly ContextManager _contexts;
    private readonly RuleManager _rules;
    private readonly WindowAssignmentManager _assignments;
    private readonly Action _onChanged;

    public Guid Id { get; }
    public string Name { get; }
    public string Icon { get; }

    [ObservableProperty]
    private int _windowCount;

    public bool CanDelete => Id != Context.UncategorizedId;

    public IRelayCommand RenameCommand { get; }
    public IRelayCommand MoveUpCommand { get; }
    public IRelayCommand MoveDownCommand { get; }
    public IRelayCommand DeleteCommand { get; }

    public ContextViewModel(
        Context context,
        int windowCount,
        ContextManager contexts,
        RuleManager rules,
        WindowAssignmentManager assignments,
        Action onChanged)
    {
        _contexts = contexts;
        _rules = rules;
        _assignments = assignments;
        _onChanged = onChanged;

        Id = context.Id;
        Name = context.Name;
        Icon = context.Icon;
        _windowCount = windowCount;

        RenameCommand = new RelayCommand(Rename);
        MoveUpCommand = new RelayCommand(() => Reorder(-1));
        MoveDownCommand = new RelayCommand(() => Reorder(1));
        DeleteCommand = new RelayCommand(Delete, () => CanDelete);
    }

    private void Rename()
    {
        var result = ContextEditWindow.Prompt("Editar contexto", Name, Icon);
        if (result is null)
        {
            return;
        }

        _contexts.Rename(Id, result.Value.Name);
        _contexts.ChangeIcon(Id, result.Value.Icon);
        _onChanged();
    }

    private void Reorder(int delta)
    {
        var ordered = _contexts.All;
        var currentIndex = ordered.ToList().FindIndex(c => c.Id == Id);
        if (currentIndex < 0)
        {
            return;
        }

        _contexts.Reorder(Id, currentIndex + delta);
        _onChanged();
    }

    private void Delete()
    {
        var confirmed = System.Windows.MessageBox.Show(
            $"Excluir o contexto \"{Name}\"? As janelas atribuídas a ele voltam para \"Sem contexto\".",
            "Smart Taskbar",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes;

        if (!confirmed)
        {
            return;
        }

        _rules.RemoveForContext(Id);
        _assignments.ReassignContext(Id, Context.UncategorizedId);
        _contexts.Delete(Id);
        _onChanged();
    }
}
