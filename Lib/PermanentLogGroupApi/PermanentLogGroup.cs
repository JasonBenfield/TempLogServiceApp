using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog;

namespace PermanentLogGroupApi
{
    public sealed class PermanentLogGroup : AppApiGroup
    {
        public PermanentLogGroup(AppApi api, IAppApiUser user, AppFactory appFactory, Clock clock)
            : base
            (
                  api,
                  new NameFromGroupClassName(nameof(PermanentLogGroup)).Value,
                  false,
                  ResourceAccess.AllowAuthenticated(),
                  user,
                  (n, a, u) => new AppApiActionCollection(n, a, u)
            )
        {
            var actions = Actions<AppApiActionCollection>();
            StartSession = actions.Add(nameof(StartSession), () => new StartSessionAction(appFactory, clock));
            StartRequest = actions.Add(nameof(StartRequest), () => new StartRequestAction(appFactory, clock));
            EndRequest = actions.Add(nameof(EndRequest), () => new EndRequestAction(appFactory, clock));
            EndSession = actions.Add(nameof(EndSession), () => new EndSessionAction(appFactory, clock));
            LogEvent = actions.Add(nameof(LogEvent), () => new LogEventAction(appFactory, clock));
            AuthenticateSession = actions.Add(nameof(AuthenticateSession), () => new AuthenticateSessionAction(appFactory, clock));
        }

        public AppApiAction<StartSessionModel, EmptyActionResult> StartSession { get; }
        public AppApiAction<StartRequestModel, EmptyActionResult> StartRequest { get; }
        public AppApiAction<EndRequestModel, EmptyActionResult> EndRequest { get; }
        public AppApiAction<EndSessionModel, EmptyActionResult> EndSession { get; }
        public AppApiAction<LogEventModel, EmptyActionResult> LogEvent { get; }
        public AppApiAction<AuthenticateSessionModel, EmptyActionResult> AuthenticateSession { get; }
    }
}
