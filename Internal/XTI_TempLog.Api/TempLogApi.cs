using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Api
{
    public sealed class TempLogApi : AppApi
    {
        public TempLogApi
        (
            AppKey appKey,
            IAppApiUser user,
            TempLogs tempLogs,
            IPermanentLogClient permanentLogClient,
            Clock clock
        )
            : base(appKey, AppVersionKey.Current, user, ResourceAccess.AllowAuthenticated())
        {
            Log = AddGroup(u => new LogGroup(this, u, tempLogs, permanentLogClient, clock));
        }

        public LogGroup Log { get; }
    }
}
