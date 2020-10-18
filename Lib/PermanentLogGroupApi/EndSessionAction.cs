using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog;

namespace PermanentLogGroupApi
{
    public sealed class EndSessionAction : AppAction<EndSessionModel, EmptyActionResult>
    {
        private readonly AppFactory appFactory;
        private readonly Clock clock;

        public EndSessionAction(AppFactory appFactory, Clock clock)
        {
            this.appFactory = appFactory;
            this.clock = clock;
        }

        public async Task<EmptyActionResult> Execute(EndSessionModel model)
        {
            var session = await appFactory.Sessions().Session(model.SessionKey);
            await session.End(clock.Now());
            return new EmptyActionResult();
        }
    }
}
