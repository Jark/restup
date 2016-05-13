using System;

namespace WebServer.Logging
{
    public interface ILogger
    {
        void Trace(string message, Exception ex);
        void Trace(string message, params object[] args);

        void Debug(string message, Exception ex);
        void Debug(string message, params object[] args);

        void Info(string message, Exception ex);
        void Info(string message, params object[] args);

        void Warn(string message, Exception ex);
        void Warn(string message, params object[] args);

        void Error(string message, Exception ex);
        void Error(string message, params object[] args);

        void Fatal(string message, Exception ex);
        void Fatal(string message, params object[] args);
    }
}