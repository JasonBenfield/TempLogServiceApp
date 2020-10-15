using System;

namespace XTI_TempLog
{
    public sealed class LogEventModel
    {
        public string EventKey { get; set; }
        public string RequestKey { get; set; }
        public int Severity { get; set; }
        public DateTime TimeOccurred { get; set; }
        public string Caption { get; set; }
        public string Message { get; set; }
        public string Detail { get; set; }
    }
}
