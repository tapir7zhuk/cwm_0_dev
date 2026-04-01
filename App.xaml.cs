using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;
using Vet_Master.Data;
using Vet_Master.Services;

namespace Vet_Master;

public partial class App : Application
{
    internal IHost _host = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, config) =>
            {
                // Явно вказуємо де знаходиться appsettings.json
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var cs = context.Configuration
                                .GetConnectionString("DefaultConnection");

                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseSqlServer(cs));

                services.AddTransient<AnimalCardService>();
                services.AddTransient<RecordService>();
                services.AddTransient<VaccinationService>();
                services.AddSingleton<PdfExportService>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        _host.Start();

        using var scope = _host.Services.CreateScope();
        scope.ServiceProvider
             .GetRequiredService<AppDbContext>()
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