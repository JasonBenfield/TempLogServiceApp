using System;

namespace XTI_TempLog
{
    public sealed class StartRequestModel
    {
        public string RequestKey { get; set; }
        public string SessionKey { get; set; }
        public string VersionKey { get; set; }
        public string Path { get; set; }
        public DateTime TimeStarted { get; set; }
    }
}
