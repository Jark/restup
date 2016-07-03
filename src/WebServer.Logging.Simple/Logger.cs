using System;
using Windows.Foundation.Diagnostics;

namespace Restup.WebServer.Logging.Simple
{
    internal class Logger : AbstractLogger
    {
        private readonly ILoggingTarget _loggingChannel;
        private readonly string _name;

        public Logger(ILoggingTarget loggingChannel, string name)
        {
            _loggingChannel = loggingChannel;
            _name = name;
        }

        protected override void LogMessage(string message, LogLevel logLevel, params object[] args)
        {
            var loggingLevel = GetLoggingLevel(logLevel);
            var formattedMessage = string.Format(message, args);
            var loggingFields = new LoggingFields();
            loggingFields.AddEmpty(formattedMessage);

            _loggingChannel.LogEvent(_name, loggingFields, loggingLevel, new LoggingOptions());
        }

        protected override void LogMessage(string message, LogLevel logLevel, Exception ex)
        {
            var loggingLevel = GetLoggingLevel(logLevel);
            var loggingFields = new LoggingFields();
            loggingFields.AddString("message", message);
            loggingFields.AddString("exception", ex.ToString());

            _loggingChannel.LogEvent(_name, loggingFields, loggingLevel, new LoggingOptions());
        }

        private static LoggingLevel GetLoggingLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    return LoggingLevel.Verbose;
                case LogLevel.Error:
                    return LoggingLevel.Error;
                case LogLevel.Info:
                    return LoggingLevel.Information;
                case LogLevel.Fatal:
                    return LoggingLevel.Critical;
                case LogLevel.Warn:
                    return LoggingLevel.Warning;
            }
            throw new ArgumentException($"Don't know how to convert {logLevel}.");
        }
    }
}
