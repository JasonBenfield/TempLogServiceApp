using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SessionLogWebApp.Api;
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
    public sealed class MoveToPermanentTest
    {
        [Test]
        public async Task ShouldStartSessionOnPermanentLog()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            fastForward(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var startSessions = input.PermanentLogClient.StartSessions();
            Assert.That(startSessions.Length, Is.EqualTo(1), "Should start session on permanent log");
            Assert.That(startSessions[0].SessionKey, Is.EqualTo(input.CurrentSession.SessionKey));
        }

        [Test]
        public async Task ShouldStartRequestOnPermanentLog()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.StartRequest("Test/Run");
            fastForward(input);
            var startRequest = await getSingleStartRequest(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var startRequests = input.PermanentLogClient.StartRequests();
            Assert.That(startRequests.Length, Is.EqualTo(1), "Should start request on permanent log");
            Assert.That(startRequests[0].RequestKey, Is.EqualTo(startRequest.RequestKey));
        }

        [Test]
        public async Task ShouldEndRequestOnPermanentLog()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.StartRequest("Test/Run");
            await input.TempSessionContext.EndRequest();
            fastForward(input);
            var startRequest = await getSingleStartRequest(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var endRequests = input.PermanentLogClient.EndRequests();
            Assert.That(endRequests.Length, Is.EqualTo(1), "Should end request on permanent log");
            Assert.That(endRequests[0].RequestKey, Is.EqualTo(startRequest.RequestKey));
        }

        [Test]
        public async Task ShouldEndSessionOnPermanentLog()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.StartRequest("Test/Run");
            await input.TempSessionContext.EndRequest();
            await input.TempSessionContext.EndSession();
            fastForward(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var endSessions = input.PermanentLogClient.EndSessions();
            Assert.That(endSessions.Length, Is.EqualTo(1), "Should end session on permanent log");
            Assert.That(endSessions[0].SessionKey, Is.EqualTo(input.CurrentSession.SessionKey));
        }

        [Test]
        public async Task ShouldAuthenticateSessionOnPermanentLog()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.StartRequest("Test/Run");
            await input.TempSessionContext.EndRequest();
            await input.TempSessionContext.AuthenticateSession("test.user");
            fastForward(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var authSessions = input.PermanentLogClient.AuthSessions();
            Assert.That(authSessions.Length, Is.EqualTo(1), "Should authenticate session on permanent log");
            Assert.That(authSessions[0].SessionKey, Is.EqualTo(input.CurrentSession.SessionKey));
        }

        [Test]
        public async Task ShouldLogEventsOnPermanentLog()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.StartRequest("Test/Run");
            try
            {
                throw new Exception("Testing");
            }
            catch (Exception ex)
            {
                await input.TempSessionContext.LogException(AppEventSeverity.Values.CriticalError, ex, "Test error");
            }
            fastForward(input);
            var startRequest = await getSingleStartRequest(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var logEvents = input.PermanentLogClient.LogEvents();
            Assert.That(logEvents.Length, Is.EqualTo(1), "Should authenticate session on permanent log");
            Assert.That(logEvents[0].RequestKey, Is.EqualTo(startRequest.RequestKey));
        }

        [Test]
        public async Task ShouldOnlyMoveFilesBeforeAMinuteAgo()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.StartRequest("Test/Run");
            await input.TempSessionContext.AuthenticateSession("test.user");
            try
            {
                throw new Exception("Testing");
            }
            catch (Exception ex)
            {
                await input.TempSessionContext.LogException(AppEventSeverity.Values.CriticalError, ex, "Test error");
            }
            await input.TempSessionContext.EndRequest();
            await input.TempSessionContext.EndSession();
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            Assert.That(input.PermanentLogClient.StartSessions().Length, Is.EqualTo(0));
            Assert.That(input.PermanentLogClient.StartRequests().Length, Is.EqualTo(0));
            Assert.That(input.PermanentLogClient.AuthSessions().Length, Is.EqualTo(0));
            Assert.That(input.PermanentLogClient.LogEvents().Length, Is.EqualTo(0));
            Assert.That(input.PermanentLogClient.EndRequests().Length, Is.EqualTo(0));
            Assert.That(input.PermanentLogClient.EndSessions().Length, Is.EqualTo(0));
        }

        [Test]
        public async Task ShouldProcessFilesOnlyOnce()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            await input.TempSessionContext.StartRequest("Test/Run");
            await input.TempSessionContext.AuthenticateSession("test.user");
            try
            {
                throw new Exception("Testing");
            }
            catch (Exception ex)
            {
                await input.TempSessionContext.LogException(AppEventSeverity.Values.CriticalError, ex, "Test error");
            }
            await input.TempSessionContext.EndRequest();
            await input.TempSessionContext.EndSession();
            fastForward(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            Assert.That(input.PermanentLogClient.StartSessions().Length, Is.EqualTo(1), "Should process start sessions once");
            Assert.That(input.PermanentLogClient.StartRequests().Length, Is.EqualTo(1), "Should process start requests once");
            Assert.That(input.PermanentLogClient.AuthSessions().Length, Is.EqualTo(1), "Should process auth sessions once");
            Assert.That(input.PermanentLogClient.LogEvents().Length, Is.EqualTo(1), "Should process log events once");
            Assert.That(input.PermanentLogClient.EndRequests().Length, Is.EqualTo(1), "Should process end requests once");
            Assert.That(input.PermanentLogClient.EndSessions().Length, Is.EqualTo(1), "Should process end sessions once");
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
