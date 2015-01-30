using System.Reflection;
using Microsoft.Practices.Prism.Logging;
using log4net;
using log4net.Config;

namespace CACCCheckInClient
{
    public class Log4NetAdapter : ILoggerFacade
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Log4NetAdapter()
        {
            XmlConfigurator.Configure();
        }

        #region ILoggerFacade Members

        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    logger.Debug(message);
                    break;
                case Category.Exception:
                    logger.Error(message);
                    break;
                case Category.Info:
                    logger.Info(message);
                    break;
                case Category.Warn:
                    logger.Warn(message);
                    break;
            }            
        }

        public void Log(Category category, Priority priority, string messageFormat, params object[] args)
        {
            Log(string.Format(messageFormat, args), category, priority);
        }

        #endregion
    }
}
