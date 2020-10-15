using System;

namespace XTI_TempLog
{
    public sealed class StartSessionModel
    {
        public string SessionKey { get; set; }
        public string UserName { get; set; }
        public string RequesterKey { get; set; }
        public DateTime? TimeStarted { get; set; }
        public DateTime? TimeEnded { get; set; }
        public string RemoteAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
