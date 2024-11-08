using Microsoft.Extensions.Logging;

namespace LarLogger.Provider
{
    public class AppLogEvents
    {
        internal static EventId Create = new EventId(1000, "Created");
        internal static EventId Read = new EventId(1001, "Read");
        internal static EventId Update = new EventId(1002, "Updated");
        internal static EventId Delete = new EventId(1003, "Deleted");

        // These are also valid EventId instances, as there's
        // an implicit conversion from int to an EventId
        internal const int Details = 3000;
        internal const int Error = 3001;

        internal static EventId ReadNotFound = 4000;
        internal static EventId UpdateNotFound = 4001;
    }
}
