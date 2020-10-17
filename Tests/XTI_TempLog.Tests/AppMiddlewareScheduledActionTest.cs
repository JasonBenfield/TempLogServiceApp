﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_ServiceApp.Extensions;
using XTI_TempLog.Fakes;

namespace XTI_TempLog.Tests
{
    public sealed class AppMiddlewareScheduledActionTest
    {
        [Test]
        public async Task ShouldLogSessionsAndRequests()
        {
            var host = await runService();
            var tempLog = host.Services.GetService<TempLog>();
            var startSessionFiles = tempLog.StartSessionFiles();
            Assert.That(startSessionFiles.Count(), Is.GreaterThanOrEqualTo(1), "Should start session");
            var startRequestFiles = tempLog.StartRequestFiles();
            Assert.That(startRequestFiles.Count(), Is.GreaterThanOrEqualTo(1), "Should start request");
            var endRequestFiles = tempLog.EndRequestFiles();
            Assert.That(endRequestFiles.Count(), Is.GreaterThanOrEqualTo(1), "Should end request");
            var endSessionFiles = tempLog.EndSessionFiles();
            Assert.That(endSessionFiles.Count(), Is.GreaterThanOrEqualTo(1), "Should end session");
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
            var clock = (FakeClock)host.Services.GetService<Clock>();
            clock.Set(new DateTime(2020, 10, 16, 13, 30, 0, DateTimeKind.Utc));
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
