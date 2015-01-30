using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using log4net;

namespace CACCCheckInServiceLibraryHost
{
    public partial class CACCCheckInService : ServiceBase
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ServiceHost host;

        public CACCCheckInService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (null != host)
            {
                host.Close();
                host = null;
            }

            logger.Debug("Creating instance of ServiceHost for CACCCheckInService.");
            host = new ServiceHost(typeof(CACCCheckInServiceLibrary.CACCCheckInService));

            logger.Debug("Opening ServiceHost to listen for client requests.");
            host.Open();
        }

        protected override void OnStop()
        {
            if (null != host)
            {
                logger.Debug("Closing ServiceHost.");
                host.Close();
            }
        }
    }
}
