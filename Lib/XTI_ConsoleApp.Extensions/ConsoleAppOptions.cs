using XTI_Schedule;

namespace XTI_ConsoleApp.Extensions
{
    public sealed class ConsoleAppOptions
    {
        public static readonly string ConsoleApp = nameof(ConsoleApp);
        public ImmediateActionOptions[] ImmediateActions { get; set; } = new ImmediateActionOptions[] { };
        public ScheduledActionOptions[] ScheduledActions { get; set; } = new ScheduledActionOptions[] { };
        public AlwaysRunningActionOptions[] AlwaysRunningActions { get; set; } = new AlwaysRunningActionOptions[] { };
    }
}
