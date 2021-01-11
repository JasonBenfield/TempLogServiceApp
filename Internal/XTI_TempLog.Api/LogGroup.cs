using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Api
{
    public sealed class LogGroup : AppApiGroup
    {
        public LogGroup
        (
            AppApi api,
            IAppApiUser user,
            TempLogs tempLogs,
            IPermanentLogClient permanentLogClient,
            Clock clock
        )
            : base
            (
                api,
                new NameFromGroupClassName(nameof(LogGroup)).Value,
                ModifierCategoryName.Default,
                api.Access,
                user,
                (n, a, u) => new AppApiActionCollection(n, a, u)
            )
        {
            var actions = Actions<AppApiActionCollection>();
            MoveToPermanent = actions.Add
            (
                nameof(MoveToPermanent),
                () => new MoveToPermanentAction(tempLogs, permanentLogClient, clock)
            );
            Retry = actions.Add
            (
                nameof(Retry),
                () => new RetryAction(tempLogs, clock)
            );
        }

        public AppApiAction<EmptyRequest, EmptyActionResult> MoveToPermanent { get; }
        public AppApiAction<EmptyRequest, EmptyActionResult> Retry { get; }
    }
}
