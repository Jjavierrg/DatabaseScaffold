
namespace DatabaseScaffold
{
    using DatabaseScaffold.Models;
    using DatabaseScaffold.ViewModels;
    using DatabaseScaffold.Views;
    using MahApps.Metro.Controls.Dialogs;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Windows;
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<ShellView>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ShellView>();
            services.AddTransient<ShellViewModel>();
            services.AddSingleton<IConsole, ConsoleExecutor>();

            services.AddTransient<IDialogCoordinator, DialogCoordinator>();
            services.AddTransient<ICommandGenerator, CommandGenerator>();

            services.AddSingleton<MotorFactory>();
            services.AddTransient<BaseMotor>();
            services.AddTransient<MotorCore5>();
        }
    }
}
