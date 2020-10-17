using System;
using System.Collections.Generic;
using System.Text;
using XTI_Core.Fakes;
using XTI_TempLog.Api;
using XTI_TempLog.Fakes;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog.Abstractions;
using SessionLogWebApp.Api;
using XTI_App.EF;
using XTI_App;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace XTI_TempLog.Tests
{
    public sealed class PermanentLogtest
    {
        [Test]
        public async Task ShouldStartSessionOnPermanentLog()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            var startSession = await getStartSession(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var session = await input.Factory.Sessions().Session(startSession.SessionKey);
            Assert.That(session.HasStarted(), Is.True, "Should start session on permanent log");
            Assert.That(session.HasEnded(), Is.False, "Should start session on permanent log");
        }

        private static async Task<StartSessionModel> getStartSession(TestInput input)
        {
            var files = input.TempLog.StartSessionFiles().ToArray();
            var serializedStartSession = await files[0].Read();
            return JsonSerializer.Deserialize<StartSessionModel>(serializedStartSession);
        }

        private TestInput setup()
        {
            var services = new ServiceCollection();
            services.AddScoped<TempLog, FakeTempLog>();
            services.AddScoped<TempSessionContext>();
            services.AddScoped<Clock, FakeClock>();
            services.AddScoped<IAppEnvironmentContext, FakeAppEnvironmentContext>();
            services.AddSingleton<CurrentSession>();
            services.AddScoped<IAppApiUser, AppApiSuperUser>();
            services.AddScoped<TempLogApi>();
            services.AddDbContext<AppDbContext>(options =>
            {
                options
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .EnableSensitiveDataLogging();
            });
            services.AddSingleton<AppFactory, EfAppFactory>();
            services.AddScoped(sp =>
            {
                var version = "Current";
                var user = sp.GetService<IAppApiUser>();
                var appFactory = sp.GetService<AppFactory>();
                var clock = sp.GetService<Clock>();
                return new SessionLogApi(version, user, appFactory, clock);
            });
            services.AddScoped<IPermanentLogClient, PermanentLogClient>();
            var sp = services.BuildServiceProvider();
            return new TestInput(sp);
        }

        private sealed class TestInput
        {
            public TestInput(IServiceProvider sp)
            {
                TempSessionContext = sp.GetService<TempSessionContext>();
                TempLog = (FakeTempLog)sp.GetService<TempLog>();
                Clock = (FakeClock)sp.GetService<Clock>();
                AppEnvironmentContext = (FakeAppEnvironmentContext)sp.GetService<IAppEnvironmentContext>();
                CurrentSession = sp.GetService<CurrentSession>();
                TempLogApi = sp.GetService<TempLogApi>();
                PermanentLogClient = (PermanentLogClient)sp.GetService<IPermanentLogClient>();
                Factory = sp.GetService<AppFactory>();
            }

            public TempSessionContext TempSessionContext { get; }
            public FakeTempLog TempLog { get; }
            public FakeClock Clock { get; }
            public FakeAppEnvironmentContext AppEnvironmentContext { get; }
            public CurrentSession CurrentSession { get; }
            public TempLogApi TempLogApi { get; }
            public PermanentLogClient PermanentLogClient { get; }
            public AppFactory Factory { get; }
        }
    }
}
