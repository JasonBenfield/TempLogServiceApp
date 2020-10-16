using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_ServiceApp.Extensions;
using XTI_TempLog.Fakes;

namespace XTI_TempLog.Tests
{
    public sealed class ScheduledActionWorkerTest
    {
        [Test]
        public async Task ShouldRunScheduledAction()
        {
            var host = BuildHost().Build();
            var clock = (FakeClock)host.Services.GetService<Clock>();
            clock.Set(new DateTime(2020, 10, 16, 13, 30, 0, DateTimeKind.Utc));
            var counter = host.Services.GetService<Counter>();
            var _ = Task.Run(() => host.Run());
            await Task.Delay(2000);
            Assert.That(counter.Value, Is.GreaterThan(0));
            Console.WriteLine($"Counter value: {counter.Value}");
            await host.StopAsync();
        }

        [Test]
        public async Task ShouldNotRunScheduledAction()
        {
            var host = BuildHost().Build();
            var clock = (FakeClock)host.Services.GetService<Clock>();
            clock.Set(new DateTime(2020, 10, 16, 14, 30, 0, DateTimeKind.Utc));
            var counter = host.Services.GetService<Counter>();
            var _ = Task.Run(() => host.RunAsync());
            await Task.Delay(2000);
            Assert.That(counter.Value, Is.EqualTo(0));
            await host.StopAsync();
        }

        private IHostBuilder BuildHost()
        {
            return Host.CreateDefaultBuilder(new string[] { })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();
                    config.AddInMemoryCollection(new[]
                    {
                        KeyValuePair.Create("ScheduledActions:0:GroupName", "Test"),
                        KeyValuePair.Create("ScheduledActions:0:ActionName", "Run"),
                        KeyValuePair.Create("ScheduledActions:0:Interval", "500"),
                        KeyValuePair.Create("ScheduledActions:0:Schedule:WeeklyTimeRanges:0:DaysOfWeek:0", "Friday"),
                        KeyValuePair.Create("ScheduledActions:0:Schedule:WeeklyTimeRanges:0:TimeRanges:0:StartTime", "900"),
                        KeyValuePair.Create("ScheduledActions:0:Schedule:WeeklyTimeRanges:0:TimeRanges:0:EndTime", "1000")
                    });
                })
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ServiceAppOptions>(hostContext.Configuration);
                    services.AddSingleton<Clock, FakeClock>();
                    services.AddSingleton<Counter>();
                    services.AddScoped<IAppApiUser, AppApiSuperUser>();
                    services.AddScoped<IAppEnvironmentContext, FakeAppEnvironmentContext>();
                    services.AddScoped<CurrentSession>();
                    services.AddScoped<TempLog, FakeTempLog>();
                    services.AddScoped<TempSessionContext>();
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
