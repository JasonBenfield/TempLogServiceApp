using PermanentLogGroupApi;
using System;
using XTI_App;
using XTI_App.Api;
using XTI_Core;

namespace SessionLogWebApp.Api
{
    public sealed class SessionLogApi : AppApi
    {
        private static readonly string AppKeyValue = "SessionLog";
        public static readonly AppKey AppKey = new AppKey(AppKeyValue);
        public SessionLogApi(string version, IAppApiUser user, AppFactory appFactory, Clock clock)
            : base
            (
                AppKeyValue,
                version,
                user,
                ResourceAccess.AllowAuthenticated()
            )
        {
            PermanentLog = AddGroup(u => new PermanentLogGroup(this, u, appFactory, clock));
        }

        public PermanentLogGroup PermanentLog { get; }
    }
}
