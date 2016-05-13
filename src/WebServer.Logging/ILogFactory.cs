namespace Restup.WebServer.Logging
{
    public interface ILogFactory
    {
        ILogger GetLogger<T>();
        ILogger GetLogger(string name);
    }
}
