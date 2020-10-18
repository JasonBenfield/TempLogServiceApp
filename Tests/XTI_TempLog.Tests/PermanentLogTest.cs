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
using XTI_WebApp.Fakes;

namespace XTI_TempLog.Tests
{
    public sealed class PermanentLogtest
    {
        [Test]
        public async Task ShouldStartSessionOnPermanentLog()
        {
            var input = await setup();
            await input.TempSessionContext.StartSession();
            var startSession = await getStartSession(input);
            fastForward(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var session = await input.Factory.Sessions().Session(startSession.SessionKey);
            Assert.That(session.HasStarted(), Is.True, "Should start session on permanent log");
            Assert.That(session.HasEnded(), Is.False, "Should start session on permanent log");
        }

        [Test]
        public async Task ShouldStartRequestOnPermanentLog()
        {
            var input = await setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.StartRequest("Test/Run");
            fastForward(input);
            var startSession = await getStartSession(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var session = await input.Factory.Sessions().Session(startSession.SessionKey);
            var requests = (await session.Requests()).ToArray();
            Assert.That(requests.Length, Is.EqualTo(1), "Should start request on permanent log");
        }

        [Test]
        public async Task ShouldEndRequestOnPermanentLog()
        {
            var input = await setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.StartRequest("Test/Run");
            await input.TempSessionContext.EndRequest();
            fastForward(input);
            var startSession = await getStartSession(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var session = await input.Factory.Sessions().Session(startSession.SessionKey);
            var requests = (await session.Requests()).ToArray();
            Assert.That(requests[0].HasEnded(), Is.True, "Should end request on permanent log");
        }

        [Test]
        public async Task ShouldEndSessionOnPermanentLog()
        {
            var input = await setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.StartRequest("Test/Run");
            await input.TempSessionContext.EndRequest();
            await input.TempSessionContext.EndSession();
            fastForward(input);
            var startSession = await getStartSession(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var session = await input.Factory.Sessions().Session(startSession.SessionKey);
            Assert.That(session.HasEnded(), Is.True, "Should end session on permanent log");
        }

        [Test]
        public async Task ShouldAuthenticateSessionOnPermanentLog()
        {
            var input = await setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.AuthenticateSession("someone");
            fastForward(input);
            var startSession = await getStartSession(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var session = await input.Factory.Sessions().Session(startSession.SessionKey);
            var user = await session.User();
            Assert.That(user.UserName, Is.EqualTo("someone"), "Should authenticate session on permanent log");
        }

        [Test]
        public async Task ShouldLogEventOnPermanentLog()
        {
            var input = await setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.StartRequest("Test/Run");
            try
            {
                throw new Exception("Test");
            }
            catch (Exception ex)
            {
                await input.TempSessionContext.LogException
                (
                    AppEventSeverity.Values.CriticalError,
                    ex,
                    "An unexpected error occurred"
                );
            }
            fastForward(input);
            var startSession = await getStartSession(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var session = await input.Factory.Sessions().Session(startSession.SessionKey);
            var requests = (await session.Requests()).ToArray();
            var events = (await requests[0].Events()).ToArray();
            Assert.That(events.Length, Is.EqualTo(1), "Should log event on permanent log");
        }

        private static async Task<StartSessionModel> getStartSession(TestInput input)
        {
            var files = input.TempLog.StartSessionFiles(input.Clock.Now()).ToArray();
            var serializedStartSession = await files[0].Read();
            return JsonSerializer.Deserialize<StartSessionModel>(serializedStartSession);
        }

        private void fastForward(TestInput input)
        {
            input.Clock.Set(input.Clock.Now().AddSeconds(61));
        }

        private async Task<TestInput> setup()
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
            var appFactory = sp.GetService<AppFactory>();
            await new AppSetup(appFactory).Run();
            var app = await appFactory.Apps().AddApp(new AppKey("Fake"), AppType.Values.WebApp, "Fake", DateTime.Now);
            var version = await app.StartNewMajorVersion(DateTime.Now);
            await version.Publishing();
            await version.Published();
            await appFactory.Users().Add(new AppUserName("test.user"), new FakeHashedPassword("Password12345"), DateTime.Now);
            await appFactory.Users().Add(new AppUserName("Someone"), new FakeHashedPassword("Password12345"), DateTime.Now);
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
