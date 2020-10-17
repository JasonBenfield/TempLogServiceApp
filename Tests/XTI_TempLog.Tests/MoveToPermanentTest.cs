using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XTI_Core.Fakes;
using XTI_TempLog.Fakes;
using Microsoft.Extensions.DependencyInjection;
using XTI_Core;
using XTI_TempLog.Api;
using XTI_App.Api;
using SessionLogWebApp.Api;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Tests
{
    public sealed class MoveToPermanentTest
    {
        [Test]
        public async Task ShouldStartSessionOnPermanentLog()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var startSessions = input.PermanentLogClient.StartSessions();
            Assert.That(startSessions.Length, Is.EqualTo(1), "Should start session on permanent log");
            Assert.That(startSessions[0].SessionKey, Is.EqualTo(input.CurrentSession.SessionKey));
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
            services.AddScoped<SessionLogApi>();
            services.AddScoped<IPermanentLogClient, FakePermanentLogClient>();
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
                PermanentLogClient = (FakePermanentLogClient)sp.GetService<IPermanentLogClient>();
            }

            public TempSessionContext TempSessionContext { get; }
            public FakeTempLog TempLog { get; }
            public FakeClock Clock { get; }
            public FakeAppEnvironmentContext AppEnvironmentContext { get; }
            public CurrentSession CurrentSession { get; }
            public TempLogApi TempLogApi { get; }
            public FakePermanentLogClient PermanentLogClient { get; }
        }
    }
}
