using System;
using System.Reflection;
using System.Windows;
using log4net;
using log4net.Config;

namespace CACCCheckInClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure log4net for use in service
            XmlConfigurator.Configure();

#if (DEBUG)
            RunInDebugMode();
#else
            RunInReleaseMode();
#endif
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    
        private static void RunInDebugMode()
        {
            logger.Debug("Creating the Bootstrapper and running.");
            Bootstrapper bootStrapper = new Bootstrapper();
            bootStrapper.Run();
        }

        private static void RunInReleaseMode()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            try
            {
                logger.Debug("Creating the Bootstrapper and running.");
                Bootstrapper bootStrapper = new Bootstrapper();
                bootStrapper.Run();
            }
            catch (Exception ex)
            {
                logger.Error("Exception caught running Bootstrapper.", ex);
                HandleException(ex);
            }
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Error("Unhandled exception detected.", e.ExceptionObject as Exception);
            HandleException(e.ExceptionObject as Exception);
        }

        private static void HandleException(Exception ex)
        {
            if (ex == null)
            { return; }

            MessageBox.Show(CACCCheckInClient.Properties.Resources.UnhandledException);
            Environment.Exit(1);
        }
    }
}
