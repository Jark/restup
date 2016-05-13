using System;

namespace WebServer.Logging.MetroLog
{
    public class Logger : ILogger
    {
        private readonly global::MetroLog.ILogger _logger;

        public Logger(global::MetroLog.ILogger logger)
        {
            _logger = logger;
        }

        public void Trace(string message, Exception ex)
        {
            _logger.Trace(message, ex);
        }

        public void Trace(string message, params object[] args)
        {
            _logger.Trace(message, args);
        }

        public void Debug(string message, Exception ex)
        {
            _logger.Debug(message, ex);
        }

        public void Debug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }

        public void Info(string message, Exception ex)
        {
            _logger.Info(message, ex);
        }

        public void Info(string message, params object[] args)
        {
            _logger.Info(message, args);
        }

        public void Warn(string message, Exception ex)
        {
            _logger.Warn(message, ex);
        }

        public void Warn(string message, params object[] args)
        {
            _logger.Warn(message, args);
        }

        public void Error(string message, Exception ex)
        {
            _logger.Error(message, ex);
        }

        public void Error(string message, params object[] args)
        {
            _logger.Error(message, args);
        }

        public void Fatal(string message, Exception ex)
        {
            _logger.Fatal(message, ex);
        }

        public void Fatal(string message, params object[] args)
        {
            _logger.Fatal(message, args);
        }
    }
}
