using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows;
using Vet_Master.Data;
using Vet_Master.Services;

namespace Vet_Master;

public partial class App : Application
{
    private IHost _host = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var cs = context.Configuration.GetConnectionString("DefaultConnection");

                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseSqlServer(cs));

                // Сервіси
                services.AddTransient<AnimalCardService>();
                services.AddTransient<RecordService>();

                // Головне вікно
                services.AddSingleton<MainWindow>();
            })
            .Build();

        _host.Start();

        // Автоматично застосувати міграції при старті
        using var scope = _host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>()
             .Database.Migrate();

        _host.Services.GetRequiredService<MainWindow>().Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}