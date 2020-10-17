using System;

namespace XTI_TempLog.Abstractions
{
    public interface IStartRequestModel
    {
        string RequestKey { get; set; }
        string SessionKey { get; set; }
        string VersionKey { get; set; }
        string Path { get; set; }
        DateTime TimeStarted { get; set; }
    }
}
