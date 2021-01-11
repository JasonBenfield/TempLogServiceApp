using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_TempLog.Abstractions;
using XTI_TempLog.Api;
using XTI_TempLog.Fakes;

namespace XTI_TempLog.Tests
{
    public sealed class RetryTest
    {
        [Test]
        public async Task ShouldResetFilesInProgress()
        {
            var input = setup();
            await input.TempSession.StartSession();
            fastForward(input);
            var startSessionFile = input.TempLog.StartSessionFiles(input.Clock.Now()).First();
            startSessionFile.WithNewName(startSessionFile.Name + ".processing");
            await input.TempLogApi.Log.Retry.Execute(new EmptyRequest());
            startSessionFile = input.TempLog.StartSessionFiles(input.Clock.Now()).FirstOrDefault();
            Assert.That(startSessionFile, Is.Not.Null, "Should reset files in progress");
        }

        private void fastForward(TestInput input)
        {
            input.Clock.Set(input.Clock.Now().AddSeconds(61));
        }

        private static async Task<StartRequestModel> getSingleStartRequest(TestInput input)
        {
            var files = input.TempLog.StartRequestFiles(DateTime.Now).ToArray();
            var serializedStartRequest = await files[0].Read();
            return JsonSerializer.Deserialize<StartRequestModel>(serializedStartRequest);
        }

        private TestInput setup()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices
                (
                    services =>
                    {
                        services.AddFakeTempLogServices();
                        services.AddScoped<TempLogs, FakeTempLogs>();
                        services.AddSingleton<Clock, FakeClock>();
                        services.AddScoped<IAppEnvironmentContext, FakeAppEnvironmentContext>();
                        services.AddScoped<IAppApiUser, AppApiSuperUser>();
                        services.AddSingleton(sp => TempLogAppKey.AppKey);
                        services.AddScoped<TempLogApi>();
                        services.AddScoped<IPermanentLogClient, FakePermanentLogClient>();
                    }
                )
                .Build();
            var scope = host.Services.CreateScope();
            return new TestInput(scope.ServiceProvider);
        }

        private sealed class TestInput
        {
            public TestInput(IServiceProvider sp)
            {
                TempSession = sp.GetService<TempLogSession>();
                TempLog = (FakeTempLog)sp.GetService<TempLog>();
                Clock = (FakeClock)sp.GetService<Clock>();
                AppEnvironmentContext = (FakeAppEnvironmentContext)sp.GetService<IAppEnvironmentContext>();
                TempLogApi = sp.GetService<TempLogApi>();
                PermanentLogClient = (FakePermanentLogClient)sp.GetService<IPermanentLogClient>();
            }

            public TempLogSession TempSession { get; }
            public FakeTempLog TempLog { get; }
            public FakeClock Clock { get; }
            public FakeAppEnvironmentContext AppEnvironmentContext { get; }
            public TempLogApi TempLogApi { get; }
            public FakePermanentLogClient PermanentLogClient { get; }
        }
    }
}
