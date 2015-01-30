using System.Reflection;
using System.ServiceProcess;
using log4net;
using log4net.Config;

namespace CACCCheckInServiceLibraryHost
{
    static class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // Configure log4net for use in service
            XmlConfigurator.Configure();

            logger.Debug("Creating instance of CACCCheckInService");
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new CACCCheckInService() 
            };
            logger.Debug("Starting instance of CACCCheckInService");
            ServiceBase.Run(ServicesToRun);
        }
    }
}
