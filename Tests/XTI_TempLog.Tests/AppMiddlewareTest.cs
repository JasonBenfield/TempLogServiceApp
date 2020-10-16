using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_ServiceApp.Extensions;
using XTI_TempLog.Fakes;

namespace XTI_TempLog.Tests
{
    public sealed class AppMiddlewareTest
    {
        [Test]
        public async Task ShouldStartSession()
        {
            var host = await runService();
            var tempLog = host.Services.GetService<TempLog>();
            var startSessionFiles = tempLog.StartSessionFiles();
            Assert.That(startSessionFiles.Count(), Is.EqualTo(1), "Should start session");
        }

        private async Task<IHost> runService()
        {
            var host = BuildHost().Build();
            return await runHost(host);
        }

        private static async Task<IHost> runHost(IHost host)
        {
            var envContext = (FakeAppEnvironmentContext)host.Services.GetService<IAppEnvironmentContext>();
            envContext.Environment = new AppEnvironment
            (
                "test.user",
                "AppMiddleware",
                "my-computer",
                "Windows 10",
                "Current"
            );
            var _ = Task.Run(() => host.StartAsync());
            var counter = host.Services.GetService<Counter>();
            while (counter.Value == 0)
            {
                await Task.Delay(100);
            }
            await host.StopAsync();
            return host;
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
                        KeyValuePair.Create("ImmediateActions:0:ActionName", "Run"),
                        KeyValuePair.Create("ImmediateActions:0:Interval", "500")
                    });
                })
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ServiceAppOptions>(hostContext.Configuration);
                    services.AddSingleton<Clock, FakeClock>();
                    services.AddSingleton<Counter>();
                    services.AddSingleton<IAppEnvironmentContext, FakeAppEnvironmentContext>();
                    services.AddSingleton<TempLog, FakeTempLog>();
                    services.AddSingleton<CurrentSession>();
                    services.AddSingleton<TempSessionContext>();
                    services.AddScoped<IAppApiUser, AppApiSuperUser>();
                    services.AddScoped<AppApi, TestApi>();
                    services.AddHostedService(sp =>
                    {
                        var options = sp.GetService<IOptions<ServiceAppOptions>>();
                        return new ServiceAppWorker
                        (
                            sp,
                            options
                        );
                    });
                });
        }
    }
}
