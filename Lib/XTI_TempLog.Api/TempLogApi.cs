using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Api
{
    public sealed class TempLogApi : AppApi
    {
        private static readonly string appKeyValue = "TempLog";
        public static readonly AppKey AppKey = new AppKey(appKeyValue);

        public TempLogApi
        (
            IAppApiUser user,
            TempLog tempLog,
            IPermanentLogClient permanentLogClient,
            Clock clock
        )
            : base(appKeyValue, "Current", user, ResourceAccess.AllowAuthenticated())
        {
            Log = AddGroup(u => new LogGroup(this, u, tempLog, permanentLogClient, clock));
        }

        public LogGroup Log { get; }
    }
}
