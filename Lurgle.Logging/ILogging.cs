namespace Lurgle.Logging
{
    public interface ILogging: IHideObjectMembers
    {
        //void Add(string logEntry);
        void Add(string logTemplate, params object[] args);
    }
}
