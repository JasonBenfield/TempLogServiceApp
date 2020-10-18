using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog;

namespace PermanentLogGroupApi
{
    public sealed class AuthenticateSessionAction : AppAction<AuthenticateSessionModel, EmptyActionResult>
    {
        private readonly AppFactory appFactory;
        private readonly Clock clock;

        public AuthenticateSessionAction(AppFactory appFactory, Clock clock)
        {
            this.appFactory = appFactory;
            this.clock = clock;
        }

        public async Task<EmptyActionResult> Execute(AuthenticateSessionModel model)
        {
            var session = await appFactory.Sessions().Session(model.SessionKey);
            var user = await appFactory.Users().User(new AppUserName(model.UserName));
            if (!user.Exists())
            {
                user = await appFactory.Users().User(AppUserName.Anon);
            }
            await session.Authenticate(user);
            return new EmptyActionResult();
        }
    }
}
