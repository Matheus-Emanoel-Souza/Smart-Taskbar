using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SmartTaskbar.App.Services;
using SmartTaskbar.App.ViewModels;
using SmartTaskbar.App.Views;
using SmartTaskbar.Core.Classification;
using SmartTaskbar.Core.Contexts;
using SmartTaskbar.Core.Persistence;
using SmartTaskbar.Core.Rules;
using SmartTaskbar.Infrastructure.Logging;
using SmartTaskbar.Infrastructure.Persistence;
using SmartTaskbar.Infrastructure.Settings;
using SmartTaskbar.Windows.ProcessManager;
using SmartTaskbar.Windows.WindowEvents;
using SmartTaskbar.Windows.WindowManager;

namespace SmartTaskbar.App;

/// <summary>
/// Raiz de composição: monta o container de DI e inicializa os serviços de fundo antes de
/// mostrar a barra. Sem <c>StartupUri</c> — a janela inicial é escolhida aqui, não no XAML,
/// porque não há "MainWindow" tradicional, só a barra flutuante.
/// </summary>
public partial class App : System.Windows.Application
{
    // Nome fixo garante que apenas uma instância rode por usuário (mutex é por sessão de logon).
    private const string SingleInstanceMutexName = "Local\\SmartTaskbar.SingleInstance.9F1E6B2A-4C3D-4F8E-9B1A-6D2E7C4A1B3F";

    private Mutex? _singleInstanceMutex;
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "O Smart Taskbar já está em execução.",
                "Smart Taskbar",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();

        var log = _services.GetRequiredService<ILogger>().ForContext<App>();

        // Circuito de proteção: um erro de binding/renderização estrutural pode se repetir a
        // cada frame e, se apenas engolido, spamma o log (visto na prática: 20MB em segundos).
        // Tolera falhas isoladas; encerra o app se a mesma exceção repetir demais em sequência.
        var lastExceptionMessage = string.Empty;
        var repeatCount = 0;
        const int maxConsecutiveRepeats = 5;

        DispatcherUnhandledException += (_, args) =>
        {
            log.Error(args.Exception, "Exceção não tratada na thread de UI");

            var message = args.Exception.Message;
            repeatCount = message == lastExceptionMessage ? repeatCount + 1 : 1;
            lastExceptionMessage = message;

            if (repeatCount >= maxConsecutiveRepeats)
            {
                log.Fatal("Mesma exceção repetiu {Count}x em sequência — encerrando para não travar em loop de erro", repeatCount);
                args.Handled = true;
                Shutdown();
                return;
            }

            args.Handled = true; // loga e mantém o app vivo em vez de derrubar a barra
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            log.Fatal(args.ExceptionObject as Exception, "Exceção não tratada fora da thread de UI");

        log.Information("Smart Taskbar iniciando");

        var orchestrator = _services.GetRequiredService<TaskbarOrchestrator>();
        orchestrator.Start();

        var barWindow = _services.GetRequiredService<BarWindow>();
        MainWindow = barWindow;
        barWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.GetService<TaskbarOrchestrator>()?.Stop();
        Log.CloseAndFlush();

        if (_singleInstanceMutex is not null)
        {
            _singleInstanceMutex.ReleaseMutex();
            _singleInstanceMutex.Dispose();
        }

        _services?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var pathProvider = new PortablePathProvider();
        var dataDirectory = pathProvider.DataDirectory;

        var logger = LoggerSetup.CreateLogger(dataDirectory);
        Log.Logger = logger;

        services.AddSingleton(pathProvider);
        services.AddSingleton(logger);
        services.AddSingleton(new SettingsService(dataDirectory));

        services.AddSingleton<IContextRepository>(new JsonContextRepository(dataDirectory));
        services.AddSingleton<IRuleRepository>(new JsonRuleRepository(dataDirectory));
        services.AddSingleton<IWindowAssignmentRepository>(new JsonWindowAssignmentRepository(dataDirectory));

        services.AddSingleton<ContextManager>();
        services.AddSingleton<RuleManager>();
        services.AddSingleton<WindowAssignmentManager>();
        services.AddSingleton<RuleEngine>();
        services.AddSingleton<WindowClassifier>();

        services.AddSingleton<ProcessInfoProvider>();
        services.AddSingleton<WindowEnumerator>();
        services.AddSingleton<WindowIconProvider>();
        services.AddSingleton<WindowActivator>();
        services.AddSingleton<WindowEventWatcher>();

        services.AddSingleton<TaskbarOrchestrator>();
        services.AddSingleton<BarViewModel>();
        services.AddSingleton<BarWindow>();
    }
}
