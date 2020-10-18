using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog;

namespace PermanentLogGroupApi
{
    public sealed class LogEventAction : AppAction<LogEventModel, EmptyActionResult>
    {
        private readonly AppFactory appFactory;
        private readonly Clock clock;

        public LogEventAction(AppFactory appFactory, Clock clock)
        {
            this.appFactory = appFactory;
            this.clock = clock;
        }

        public async Task<EmptyActionResult> Execute(LogEventModel model)
        {
            var request = await appFactory.Requests().Request(model.RequestKey);
            var severity = AppEventSeverity.Values.Value(model.Severity);
            await request.LogEvent
            (
                model.EventKey,
                severity,
                clock.Now(),
                model.Caption,
                model.Message,
                model.Detail
            );
            return new EmptyActionResult();
        }
    }
}
