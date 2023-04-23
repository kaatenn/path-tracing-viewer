using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace _3D_viewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Build configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            // Build service provider
            var serviceProvider = new ServiceCollection()
                .AddDbContext<App_Db_Context>(options =>
                    options.UseMySql(
                        configuration.GetConnectionString("DefaultConnection"),
                        new MySqlServerVersion(new Version(8, 0, 30))), ServiceLifetime.Transient)
                .BuildServiceProvider();

            // Create database if not exists
            using (var context = serviceProvider.GetService<App_Db_Context>())
            {
                context.Database.EnsureCreated();
            }
        }
    }
}
