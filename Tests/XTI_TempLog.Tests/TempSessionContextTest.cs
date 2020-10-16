using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_TempLog.Fakes;

namespace XTI_TempLog.Tests
{
    public sealed class TempSessionContextTest
    {
        [Test]
        public async Task ShouldWriteToLog_WhenStartingASession()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            var files = input.
                TempLog.Files();
            Assert.That(files.Length, Is.EqualTo(1));
        }

        [Test]
        public async Task ShouldWriteSessionToLog_WhenStartingASession()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            var files = input.TempLog.StartSessionFiles().ToArray();
            var serializedSession = await files[0].Read();
            var tempSession = JsonSerializer.Deserialize<StartSessionModel>(serializedSession);
            Assert.That(string.IsNullOrWhiteSpace(tempSession.SessionKey), Is.False, "Should create session key");
            Assert.That(tempSession.SessionKey, Is.EqualTo(input.CurrentSession.SessionKey), "Should set current session key");
            Assert.That(tempSession.TimeStarted, Is.EqualTo(input.Clock.Now()), "Should start session");
            Assert.That(tempSession.UserName, Is.EqualTo(input.AppEnvironmentContext.Environment.UserName), "Should set user name from environment");
            Assert.That(tempSession.RequesterKey, Is.EqualTo(input.AppEnvironmentContext.Environment.RequesterKey), "Should set requester key from environment");
            Assert.That(tempSession.UserAgent, Is.EqualTo(input.AppEnvironmentContext.Environment.UserAgent), "Should set user agent from environment");
            Assert.That(tempSession.RemoteAddress, Is.EqualTo(input.AppEnvironmentContext.Environment.RemoteAddress), "Should set remote address from environment");
        }

        [Test]
        public async Task ShouldWriteRequestToLog_WhenStartingARequest()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            var path = "group1/action1";
            await input.TempSessionContext.StartRequest(path);
            var files = input.TempLog.StartRequestFiles().ToArray();
            Assert.That(files.Length, Is.EqualTo(1));
            var serializedSession = await files[0].Read();
            var tempRequest = JsonSerializer.Deserialize<StartRequestModel>(serializedSession);
            Assert.That(tempRequest.SessionKey, Is.EqualTo(input.CurrentSession.SessionKey), "Should create session key");
            Assert.That(tempRequest.TimeStarted, Is.EqualTo(input.Clock.Now()), "Should start session");
            Assert.That(tempRequest.Path, Is.EqualTo(path), "Should set path");
            Assert.That(string.IsNullOrWhiteSpace(tempRequest.RequestKey), Is.False, "Should set request key");
            Assert.That(tempRequest.VersionKey, Is.EqualTo(input.AppEnvironmentContext.Environment.VersionKey), "Should set version key from environment");
        }

        [Test]
        public async Task ShouldLogError()
        {
            var input = setup();
            await input.TempSessionContext.StartSession();
            var path = "group1/action1";
            await input.TempSessionContext.StartRequest(path);
            Exception thrownException;
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
                thrownException = ex;
            }
            var requestFiles = input.TempLog.StartRequestFiles().ToArray();
            var request = JsonSerializer.Deserialize<StartRequestModel>(await requestFiles[0].Read());
            var eventFiles = input.TempLog.LogEventFiles().ToArray();
            Assert.That(eventFiles.Length, Is.EqualTo(1));
            var serializedSession = await eventFiles[0].Read();
            var tempEvent = JsonSerializer.Deserialize<LogEventModel>(serializedSession);
            Assert.That(string.IsNullOrWhiteSpace(tempEvent.EventKey), Is.False, "Should create event key");
            Assert.That(tempEvent.RequestKey, Is.EqualTo(request.RequestKey), "Should set request key");
            Assert.That(tempEvent.Severity, Is.EqualTo(AppEventSeverity.Values.CriticalError.Value), "Should set severity");
            Assert.That(tempEvent.Caption, Is.EqualTo("An unexpected error occurred"), "Should set caption");
            Assert.That(tempEvent.Message, Is.EqualTo(thrownException.Message), "Should set message");
            Assert.That(tempEvent.Detail, Is.EqualTo(thrownException.StackTrace), "Should set detail");
            Assert.That(tempEvent.TimeOccurred, Is.EqualTo(input.Clock.Now()), "Should set time occurred to the current time");
        }

        private TestInput setup()
        {
            var services = new ServiceCollection();
            services.AddScoped<TempLog, FakeTempLog>();
            services.AddScoped<TempSessionContext>();
            services.AddScoped<Clock, FakeClock>();
            services.AddScoped<IAppEnvironmentContext>(sp => new FakeAppEnvironmentContext
            {
                Environment = new AppEnvironment
                (
                    "test.user", "my-computer", "10.1.0.0", "Windows 10", "V1"
                )
            });
            services.AddSingleton<CurrentSession>();
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
            }

            public TempSessionContext TempSessionContext { get; }
            public FakeTempLog TempLog { get; }
            public FakeClock Clock { get; }
            public FakeAppEnvironmentContext AppEnvironmentContext { get; }
            public CurrentSession CurrentSession { get; }
        }
    }
}
