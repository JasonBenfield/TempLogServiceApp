namespace XTI_TempLog.Api
{
    public sealed class LogOptions
    {
        public static readonly string Log = "Log";

        public int ProcessMinutesBefore { get; set; } = 1;
    }
}
