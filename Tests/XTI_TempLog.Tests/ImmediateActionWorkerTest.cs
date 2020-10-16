using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_ConsoleApp.Extensions;
using XTI_ServiceApp.Extensions;

namespace XTI_TempLog.Tests
{
    public sealed class ImmediateActionWorkerTest
    {
        private static readonly Counter counter = new Counter();

        [Test]
        public async Task ShouldRunImmediateAction()
        {
            await BuildHost().RunConsoleAsync();
            Assert.That(counter.Value, Is.EqualTo(1));
        }

        private IHostBuilder BuildHost()
        {
            return Host.CreateDefaultBuilder(new string[] { })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();
                    config.AddInMemoryCollection(new[]
                    {
                        KeyValuePair.Create("ImmediateActions:0:GroupName", "Test"),
                        KeyValuePair.Create("ImmediateActions:0:ActionName", "Run")
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ConsoleAppOptions>(hostContext.Configuration);
                    services.AddSingleton(sp => counter);
                    services.AddScoped<IAppApiUser, AppApiSuperUser>();
                    services.AddScoped<AppApi, TestApi>();
                    services.AddHostedService(sp =>
                    {
                        var options = sp.GetService<IOptions<ConsoleAppOptions>>();
                        return new ConsoleAppWorker
                        (
                            sp,
                            options
                        );
                    });
                });
        }
    }
}
