using XTI_App;
using XTI_App.Api;

namespace XTI_TempLog.Api
{
    public sealed class TempLogApi : AppApi
    {
        private static readonly string appKeyValue = "TempLog";
        public static readonly AppKey AppKey = new AppKey(appKeyValue);

        public TempLogApi(IAppApiUser user)
            : base(appKeyValue, "Current", user, ResourceAccess.AllowAuthenticated())
        {
            Log = AddGroup(u => new LogGroup(this, u));
        }

        public LogGroup Log { get; }
    }
}
