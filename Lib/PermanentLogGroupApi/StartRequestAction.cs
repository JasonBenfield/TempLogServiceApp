using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog;

namespace PermanentLogGroupApi
{
    public sealed class StartRequestAction : AppAction<StartRequestModel, EmptyActionResult>
    {
        private readonly AppFactory appFactory;
        private readonly Clock clock;

        public StartRequestAction(AppFactory appFactory, Clock clock)
        {
            this.appFactory = appFactory;
            this.clock = clock;
        }

        public async Task<EmptyActionResult> Execute(StartRequestModel model)
        {
            var session = await appFactory.Sessions().Session(model.SessionKey);
            var versionKey = AppVersionKey.Parse(model.VersionKey);
            var appKey = new AppKey(model.AppKey);
            var appType = AppType.Values.Value(model.AppType);
            var app = await appFactory.Apps().App(appKey, appType);
            var version = await app.Version(versionKey);
            await session.LogRequest(model.RequestKey, version, model.Path, clock.Now());
            return new EmptyActionResult();
        }
    }
}
