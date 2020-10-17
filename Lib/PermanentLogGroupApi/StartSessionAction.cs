using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog;

namespace PermanentLogGroupApi
{
    public sealed class StartSessionAction : AppAction<StartSessionModel, EmptyActionResult>
    {
        private readonly AppFactory appFactory;
        private readonly Clock clock;

        public StartSessionAction(AppFactory appFactory, Clock clock)
        {
            this.appFactory = appFactory;
            this.clock = clock;
        }

        public async Task<EmptyActionResult> Execute(StartSessionModel model)
        {
            var user = await appFactory.Users().User(new AppUserName(model.UserName));
            if (!user.Exists())
            {
                user = await appFactory.Users().User(AppUserName.Anon);
            }
            var timeStarted = clock.Now();
            await appFactory.Sessions().Create
            (
                model.SessionKey,
                user,
                timeStarted,
                model.RequesterKey,
                model.UserAgent,
                model.RemoteAddress
            );
            return new EmptyActionResult();
        }
    }
}
