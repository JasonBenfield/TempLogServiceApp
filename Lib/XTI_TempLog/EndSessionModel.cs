using System;

namespace XTI_TempLog
{
    public sealed class EndSessionModel
    {
        public string SessionKey { get; set; }
        public DateTime TimeEnded { get; set; }
    }
}
