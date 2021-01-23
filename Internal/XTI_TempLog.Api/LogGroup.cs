using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Api
{
    public sealed class LogGroup : AppApiGroupWrapper
    {
        public LogGroup
        (
            AppApiGroup source,
            LogActionFactory actionFactory
        )
            : base(source)
        {
            var actions = new AppApiActionFactory(source);
            MoveToPermanent = source.AddAction
            (
                actions.Action
                (
                    nameof(MoveToPermanent),
                    actionFactory.CreateMoveToPermanent
                )
            );
            Retry = source.AddAction
            (
                actions.Action
                (
                    nameof(Retry),
                    actionFactory.CreateRetry
                )
            );
        }

        public AppApiAction<EmptyRequest, EmptyActionResult> MoveToPermanent { get; }
        public AppApiAction<EmptyRequest, EmptyActionResult> Retry { get; }
    }
}
