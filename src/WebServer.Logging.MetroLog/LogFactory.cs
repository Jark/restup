using MetroLog;
using ILogger = Restup.WebServer.Logging.ILogger;

namespace Restup.WebServer.Logging.MetroLog
{
    public class LogFactory : ILogFactory
    {
        private readonly LoggingConfiguration _loggingConfiguration;
        public LogFactory(LoggingConfiguration loggingConfiguration = null)
        {
            _loggingConfiguration = loggingConfiguration;
        }

        public ILogger GetLogger<T>()
        {
            var logger = LogManagerFactory.DefaultLogManager.GetLogger<T>(_loggingConfiguration);
            return new Logger(logger);
        }

        public ILogger GetLogger(string name)
        {
            var logger = LogManagerFactory.DefaultLogManager.GetLogger<Logger>(_loggingConfiguration);
            return new Logger(logger);
        }
    }
}