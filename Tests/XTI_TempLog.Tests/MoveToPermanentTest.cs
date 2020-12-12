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
    public sealed class MoveToPermanentTest
    {
        [Test]
        public async Task ShouldStartSessionOnPermanentLog()
        {
            var input = setup();
            var session = await input.TempSession.StartSession();
            fastForward(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var startSessions = input.PermanentLogClient.StartSessions();
            Assert.That(startSessions.Length, Is.EqualTo(1), "Should start session on permanent log");
            Assert.That(startSessions[0].SessionKey, Is.EqualTo(session.SessionKey));
        }

        [Test]
        public async Task ShouldStartRequestOnPermanentLog()
        {
            var input = setup();
            await input.TempSession.StartSession();
            await input.TempSession.StartRequest("Test/Run");
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
            await input.TempSession.StartSession();
            await input.TempSession.StartRequest("Test/Run");
            await input.TempSession.EndRequest();
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
            var session = await input.TempSession.StartSession();
            await input.TempSession.StartRequest("Test/Run");
            await input.TempSession.EndRequest();
            await input.TempSession.EndSession();
            fastForward(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var endSessions = input.PermanentLogClient.EndSessions();
            Assert.That(endSessions.Length, Is.EqualTo(1), "Should end session on permanent log");
            Assert.That(endSessions[0].SessionKey, Is.EqualTo(session.SessionKey));
        }

        [Test]
        public async Task ShouldAuthenticateSessionOnPermanentLog()
        {
            var input = setup();
            await input.TempSession.StartSession();
            await input.TempSession.StartRequest("Test/Run");
            await input.TempSession.EndRequest();
            await input.TempSession.AuthenticateSession("test.user");
            fastForward(input);
            await input.TempLogApi.Log.MoveToPermanent.Execute(new EmptyRequest());
            var authSessions = input.PermanentLogClient.AuthSessions();
            Assert.That(authSessions.Length, Is.EqualTo(1), "Should authenticate session on permanent log");
            Assert.That(authSessions[0].UserName, Is.EqualTo(input.AppEnvironmentContext.Environment.UserName));
        }

        [Test]
        public async Task ShouldLogEventsOnPermanentLog()
        {
            var input = setup();
            await input.TempSession.StartSession();
            await input.TempSession.StartRequest("Test/Run");
            try
            {
                throw new Exception("Testing");
            }
            catch (Exception ex)
            {
                await input.TempSession.LogException(AppEventSeverity.Values.CriticalError, ex, "Test error");
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
            await input.TempSession.StartSession();
            await input.TempSession.StartRequest("Test/Run");
            await input.TempSession.AuthenticateSession("test.user");
            try
            {
                throw new Exception("Testing");
            }
            catch (Exception ex)
            {
                await input.TempSession.LogException(AppEventSeverity.Values.CriticalError, ex, "Test error");
            }
            await input.TempSession.EndRequest();
            await input.TempSession.EndSession();
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
            await input.TempSession.StartSession();
            await input.TempSession.StartRequest("Test/Run");
            await input.TempSession.AuthenticateSession("test.user");
            try
            {
                throw new Exception("Testing");
            }
            catch (Exception ex)
            {
                await input.TempSession.LogException(AppEventSeverity.Values.CriticalError, ex, "Test error");
            }
            await input.TempSession.EndRequest();
            await input.TempSession.EndSession();
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
