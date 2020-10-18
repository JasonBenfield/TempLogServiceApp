using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog;

namespace PermanentLogGroupApi
{
    public sealed class EndRequestAction : AppAction<EndRequestModel, EmptyActionResult>
    {
        private readonly AppFactory appFactory;
        private readonly Clock clock;

        public EndRequestAction(AppFactory appFactory, Clock clock)
        {
            this.appFactory = appFactory;
            this.clock = clock;
        }

        public async Task<EmptyActionResult> Execute(EndRequestModel model)
        {
            var request = await appFactory.Requests().Request(model.RequestKey);
            await request.End(clock.Now());
            return new EmptyActionResult();
        }
    }
}
