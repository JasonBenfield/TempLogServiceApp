using XTI_Schedule;

namespace XTI_ServiceApp.Extensions
{
    public sealed class ServiceAppOptions
    {
        public static readonly string ServiceApp = nameof(ServiceApp);
        public ImmediateActionOptions[] ImmediateActions { get; set; } = new ImmediateActionOptions[] { };
        public ScheduledActionOptions[] ScheduledActions { get; set; } = new ScheduledActionOptions[] { };
        public AlwaysRunningActionOptions[] AlwaysRunningActions { get; set; } = new AlwaysRunningActionOptions[] { };
    }
}
