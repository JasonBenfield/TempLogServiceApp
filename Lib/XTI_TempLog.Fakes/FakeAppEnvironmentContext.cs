namespace XTI_TempLog.Fakes
{
    public sealed class FakeAppEnvironmentContext : IAppEnvironmentContext
    {
        public FakeAppEnvironmentContext()
        {
            Environment = new AppEnvironment
            (
                "test.user",
                "AppMiddleware",
                "my-computer",
                "Windows 10",
                "Fake",
                "Current"
            );
        }

        public AppEnvironment Environment { get; set; }

        public AppEnvironment Value() => Environment;
    }
}
