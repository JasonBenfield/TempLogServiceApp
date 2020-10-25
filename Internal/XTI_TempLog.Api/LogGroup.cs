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
            TempLog tempLog,
            IPermanentLogClient permanentLogClient,
            Clock clock
        )
            : base
            (
                api,
                new NameFromGroupClassName(nameof(LogGroup)).Value,
                false,
                api.Access,
                user,
                (n, a, u) => new AppApiActionCollection(n, a, u)
            )
        {
            var actions = Actions<AppApiActionCollection>();
            MoveToPermanent = actions.Add
            (
                nameof(MoveToPermanent),
                () => new MoveToPermanentAction(tempLog, permanentLogClient, clock)
            );
        }

        public AppApiAction<EmptyRequest, EmptyActionResult> MoveToPermanent { get; }
    }
}
