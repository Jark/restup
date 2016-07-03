using System;
using Windows.Foundation.Diagnostics;

namespace Restup.WebServer.Logging.Simple
{
    public class LogFactory : ILogFactory
    {
        private readonly LoggingChannel _loggingChannel;
        private readonly FileLoggingSession _loggingFileSession;

        public static Guid LoggingChannelId = new Guid("deea9179-380f-445f-8709-28b2e296a9ae");

        public LogFactory(string name = "Restup.WebServer", LoggingChannelOptions options = null, string fileLoggingSessionName = null)
        {            
            _loggingChannel = new LoggingChannel(name, options, LoggingChannelId);
            if (!string.IsNullOrWhiteSpace(fileLoggingSessionName))
            {
                _loggingFileSession = GetFileLoggingSession(_loggingChannel, fileLoggingSessionName);
            }
        }

        private static FileLoggingSession GetFileLoggingSession(ILoggingChannel loggingChannel, string fileLoggingSessionName)
        {
            var loggingFileSession = new FileLoggingSession(fileLoggingSessionName);
            loggingFileSession.AddLoggingChannel(loggingChannel);
            return loggingFileSession;
        }

        public ILogger GetLogger<T>()
        {
            return GetLogger(typeof(T).Name);
        }

        public ILogger GetLogger(string name)
        {
            return new Logger(_loggingChannel, name);
        }

        public void Dispose()
        {
            try
            {
                _loggingFileSession?.Dispose();
            }
            catch { 
                // don't care if this throws since we'll be shutting down the server
            }

            try
            {
                _loggingChannel.Dispose();
            }
            catch
            {
                // don't care if this throws since we'll be shutting down the server
            }
        }
    }
}