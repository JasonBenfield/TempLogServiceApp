using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_TempLog.Tests
{
    public sealed class Counter
    {
        public int Value { get; private set; }

        public void Increment()
        {
            Value++;
        }
    }
    public static class TestAppKey
    {
        public static readonly AppKey AppKey = new AppKey("Test", AppType.Values.WebApp);
    }
    public sealed class TestApi : AppApiWrapper
    {
        public TestApi(IAppApiUser user, Counter counter)
            : base(new AppApi(TestAppKey.AppKey, user, ResourceAccess.AllowAuthenticated()))
        {
            Test = new TestGroup(source.AddGroup(nameof(Test)), counter);
        }

        public TestGroup Test { get; }
    }

    public sealed class TestGroup : AppApiGroupWrapper
    {
        public TestGroup(AppApiGroup source, Counter counter)
            : base(source)
        {
            var actions = new AppApiActionFactory(source);
            Run = source.AddAction
            (
                actions.Action
                (
                    nameof(Run),
                    () => new RunAction(counter)
                )
            );
        }

        public AppApiAction<EmptyRequest, EmptyActionResult> Run { get; }
    }

    public sealed class RunAction : AppAction<EmptyRequest, EmptyActionResult>
    {
        private readonly Counter counter;

        public RunAction(Counter counter)
        {
            this.counter = counter;
        }

        public Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            counter.Increment();
            return Task.FromResult(new EmptyActionResult());
        }
    }

}
