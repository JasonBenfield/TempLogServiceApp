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
        }

        public AppApiAction<StartSessionModel, EmptyActionResult> StartSession { get; }
    }
}
