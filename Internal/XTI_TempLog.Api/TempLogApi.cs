using System;
using XTI_App.Api;

namespace XTI_TempLog.Api
{
    public sealed class TempLogApi : AppApiWrapper
    {
        public TempLogApi
        (
            IAppApiUser user,
            IServiceProvider services
        )
            : base
            (
                new AppApi
                (
                    TempLogAppKey.AppKey,
                    user,
                    ResourceAccess.AllowAuthenticated()
                )
            )
        {
            Log = new LogGroup
            (
                source.AddGroup(nameof(Log)),
                new LogActionFactory(services)
            );
        }

        public LogGroup Log { get; }
    }
}
