using System;

namespace Restup.WebServer.Logging
{
    public abstract class AbstractLogger : ILogger
    {
        public void Trace(string message, Exception ex) => LogMessage(message, LogLevel.Trace, ex);
        public void Trace(string message, params object[] args) => LogMessage(message, LogLevel.Trace, args);
        public void Debug(string message, Exception ex) => LogMessage(message, LogLevel.Debug, ex);
        public void Debug(string message, params object[] args) => LogMessage(message, LogLevel.Debug, args);
        public void Info(string message, Exception ex) => LogMessage(message, LogLevel.Info, ex);
        public void Info(string message, params object[] args) => LogMessage(message, LogLevel.Info, args);
        public void Warn(string message, Exception ex) => LogMessage(message, LogLevel.Warn, ex);
        public void Warn(string message, params object[] args) => LogMessage(message, LogLevel.Warn, args);
        public void Error(string message, Exception ex) => LogMessage(message, LogLevel.Error, ex);
        public void Error(string message, params object[] args) => LogMessage(message, LogLevel.Error, args);
        public void Fatal(string message, Exception ex) => LogMessage(message, LogLevel.Fatal, ex);
        public void Fatal(string message, params object[] args) => LogMessage(message, LogLevel.Fatal, args);

        protected abstract void LogMessage(string message, LogLevel loggingLevel, params object[] args);
        protected abstract void LogMessage(string message, LogLevel loggingLevel, Exception ex);

        protected enum LogLevel
        {
            Trace,
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }
    }
}