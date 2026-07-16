using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AnumoDriver.Controllers;

namespace AnumoDriver;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static string LocalDataFolder { get; set; }

    public static ServiceProvider? Service;
    private ServiceProvider _service { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        LocalDataFolder = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Name
        );

        var sc = new ServiceCollection();
        sc.AddSingleton<InternalSave>();
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
