using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Core;

namespace XTI_TempLog.Api
{
    public sealed class TempLogSetup
    {
        private readonly AppFactory factory;
        private readonly Clock clock;
        private readonly AppApiFactory apiFactory;

        public TempLogSetup(AppFactory factory, Clock clock, AppApiFactory apiFactory)
        {
            this.factory = factory;
            this.clock = clock;
            this.apiFactory = apiFactory;
        }

        public async Task Run()
        {
            await new AllAppSetup(factory, clock).Run();
            await new DefaultAppSetup
            (
                factory,
                clock,
                apiFactory.CreateTemplate(),
                "Temp Log"
            ).Run();
        }
    }
}
