using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AnumoDriver.Controllers;

namespace AnumoDriver;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static ServiceProvider? Service;
    private ServiceProvider _service { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<SerialCommunicator>();
        sc.AddSingleton<InputController>();
        sc.AddSingleton<AppFocusHandler>();
        sc.AddSingleton<GlobalTick>();
        sc.AddSingleton<MainWindow>(); // register windows/pages via DI
        _service = sc.BuildServiceProvider();

        var tickService = _service.GetRequiredService<GlobalTick>();
        tickService.Start();

        var main = _service.GetRequiredService<MainWindow>();
        main.Show();

        Service = _service;

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Service?.Dispose();
        base.OnExit(e);
    }
}
