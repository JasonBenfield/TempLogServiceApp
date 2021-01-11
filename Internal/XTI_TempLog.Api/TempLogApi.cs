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
            IAppApiUser user,
            TempLogs tempLogs,
            IPermanentLogClient permanentLogClient,
            Clock clock
        )
            : base(TempLogAppKey.AppKey, user, ResourceAccess.AllowAuthenticated())
        {
            Log = AddGroup(u => new LogGroup(this, u, tempLogs, permanentLogClient, clock));
        }

        public LogGroup Log { get; }
    }
}
