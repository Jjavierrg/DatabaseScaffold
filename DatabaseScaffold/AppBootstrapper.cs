namespace DatabaseScaffold
{
    using Caliburn.Micro;
    using DatabaseScaffold.Interfaces;
    using DatabaseScaffold.Models;
    using DatabaseScaffold.ViewModels;
    using MahApps.Metro.Controls.Dialogs;
    using System;
    using System.Collections.Generic;
    using System.Windows;

    public class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public AppBootstrapper() => Initialize();

        protected override void Configure()
        {
            _container = new SimpleContainer();

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();

            _container.PerRequest<IShell, ShellViewModel>();
            _container.PerRequest<IDialogCoordinator, DialogCoordinator>();
            _container.PerRequest<ICommandGenerator, CommandGenerator>();
        }

        protected override object GetInstance(Type service, string key) => _container.GetInstance(service, key);

        protected override IEnumerable<object> GetAllInstances(Type service) => _container.GetAllInstances(service);

        protected override void BuildUp(object instance) => _container.BuildUp(instance);

        protected override void OnStartup(object sender, StartupEventArgs e) => DisplayRootViewFor<IShell>();
    }
}
