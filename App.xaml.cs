using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using TillApp.Infrastructure;
using TillApp.Services;
using TillApp.ViewModels;

namespace TillApp
{
    public partial class App : Application
    {
        private IHost _host = null!;
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(RegisterServices)
                .Build();
            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
                await DatabaseSeeder.SeedAsync(db);
            }

            _host.Services.GetRequiredService<MainWindow>().Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
            base.OnExit(e);
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<MainWindow>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<ProductListViewModel>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISessionService, SessionService>();
            services.AddValidatorsFromAssemblyContaining<App>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<App>();
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=tillapp.db"));
        }
    }
}
