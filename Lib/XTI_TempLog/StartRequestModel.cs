using System;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog
{
    public sealed class StartRequestModel : IStartRequestModel
    {
        public string RequestKey { get; set; }
        public string SessionKey { get; set; }
        public string AppKey { get; set; }
        public int AppType { get; set; }
        public string VersionKey { get; set; }
        public string Path { get; set; }
        public DateTime TimeStarted { get; set; }
    }
}
