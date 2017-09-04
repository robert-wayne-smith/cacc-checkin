using System.Windows;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;

namespace CACCCheckInClient
{
    using CACCCheckIn.Modules.Children;
    using CACCCheckIn.Modules.Preschool;
    using CACCCheckIn.Modules.MOPS;
    using CACCCheckIn.Modules.LadiesBibleStudy;
    using CACCCheckIn.Modules.Admin;
    using Microsoft.Practices.Unity;
    using ServiceAndDataContracts;
    using CACCCheckInClient.Services;

    public class Bootstrapper : UnityBootstrapper
    {
        //private readonly Log4NetAdapter _logger = new Log4NetAdapter();

        protected override DependencyObject CreateShell()
        {
            Shell shell = this.Container.Resolve<Shell>();

            if (shell != null)
            {
                shell.Show();
            }

            return shell;
        }

        //protected override ILoggerFacade LoggerFacade
        //{
        //    get { return _logger; }
        //}

        /// <summary>
        /// Loads the modules needed by this application
        /// </summary>
        /// <returns></returns>
        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ModuleCatalog()
                 .AddModule(typeof(ChildrenModule))
                 .AddModule(typeof(PreschoolModule))
                 .AddModule(typeof(MOPSModule))
                 .AddModule(typeof(LadiesBibleStudyModule))
                 .AddModule(typeof(AdminModule));
        }
        
        /// <summary>
        /// Loads the services used by modules in this application
        /// </summary>
        protected override void ConfigureContainer()
        {
            Container.RegisterType<IConfigurationService, ConfigurationService>(
                new ContainerControlledLifetimeManager());
            Container.RegisterType<ILabelPrinterService, LabelPrinterService>(
                new ContainerControlledLifetimeManager());

            base.ConfigureContainer();
        }
    }
}
