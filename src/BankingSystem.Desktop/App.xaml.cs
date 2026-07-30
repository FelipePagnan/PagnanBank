using System.Globalization;
using System.IO;
using System.Windows;
using BankingSystem.Application;
using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Desktop.Services;
using BankingSystem.Desktop.Session;
using BankingSystem.Desktop.ViewModels;
using BankingSystem.Desktop.Views;
using BankingSystem.Infrastructure;
using BankingSystem.Persistence;
using BankingSystem.Persistence.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BankingSystem.Desktop;

public partial class App : System.Windows.Application
{
    private IHost? _host;
    private IServiceScope? _scope;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var culture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        // QuestPDF free Community license (required before generating documents).
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, "logs", "log-.txt"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
            {
                ContentRootPath = AppContext.BaseDirectory
            });

            builder.Logging.ClearProviders();
            builder.Services.AddSerilog(Log.Logger);

            var connectionString = builder.Configuration.GetConnectionString("Default")
                                    ?? "Data Source=banking.db";

            ConfigureServices(builder.Services, connectionString);

            _host = builder.Build();
            await _host.StartAsync();

            // Seed once (idempotent) in a temporary scope.
            using (var seedScope = _host.Services.CreateScope())
            {
                await seedScope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
            }

            // We drive shutdown explicitly so that logging out can return to the
            // login screen instead of closing the whole application.
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            RunSessionLoop();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Falha na inicialização da aplicação.");
            MessageBox.Show(
                "Não foi possível iniciar o sistema:\n\n" + ex.Message,
                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    /// <summary>
    /// Runs the login -> shell cycle. Logging out closes the shell and loops back
    /// to the login window with a brand-new DI scope (fresh DbContext/session).
    /// </summary>
    private void RunSessionLoop()
    {
        while (true)
        {
            _scope?.Dispose();
            _scope = _host!.Services.CreateScope();
            var provider = _scope.ServiceProvider;

            var login = provider.GetRequiredService<LoginWindow>();
            if (login.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            var shell = provider.GetRequiredService<ShellWindow>();
            MainWindow = shell;

            // ShowDialog returns true only when the user chose "Sair" (logout).
            // Closing the window (X) returns null/false and exits the application.
            var logoutRequested = shell.ShowDialog() == true;
            if (!logoutRequested)
            {
                Shutdown();
                return;
            }
            // loop -> back to the login window
        }
    }

    private static void ConfigureServices(IServiceCollection services, string connectionString)
    {
        // Layered composition root.
        services.AddInfrastructure();
        services.AddPersistence(connectionString);
        services.AddApplication();

        // Presentation session (also exposed to Application via ICurrentUser).
        services.AddSingleton<UserSession>();
        services.AddSingleton<ICurrentUser>(sp => sp.GetRequiredService<UserSession>());
        services.AddSingleton<IDialogService, DialogService>();

        // View models.
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<StatementViewModel>();
        services.AddTransient<NewTransactionViewModel>();
        services.AddTransient<InvestmentsViewModel>();
        services.AddTransient<LoansViewModel>();
        services.AddTransient<CardsViewModel>();
        services.AddTransient<StoreViewModel>();
        services.AddTransient<AdminUsersViewModel>();
        services.AddTransient<AuditViewModel>();
        services.AddTransient<LoginHistoryViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<SimulatorsViewModel>();

        // Windows.
        services.AddTransient<LoginWindow>();
        services.AddTransient<RegisterWindow>();
        services.AddTransient<ShellWindow>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            _scope?.Dispose();
            if (_host is not null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }
        finally
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
