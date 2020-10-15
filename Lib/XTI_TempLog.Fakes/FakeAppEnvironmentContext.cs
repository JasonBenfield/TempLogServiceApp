namespace XTI_TempLog.Fakes
{
    public sealed class FakeAppEnvironmentContext : IAppEnvironmentContext
    {
        public AppEnvironment Environment { get; set; }

        public AppEnvironment Value() => Environment;
    }
}
