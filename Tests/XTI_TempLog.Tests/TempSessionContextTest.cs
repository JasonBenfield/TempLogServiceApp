using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var files = input.TempLog.Files();
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
            Assert.That(tempSession.SessionKey, Is.Not.EqualTo(""), "Should create session key");
            Assert.That(tempSession.TimeStarted, Is.EqualTo(input.Clock.Now()), "Should start session");
            Assert.That(tempSession.UserName, Is.EqualTo(input.AppEnvironmentContext.Environment.UserName), "Should set user name from environment");
            Assert.That(tempSession.RequesterKey, Is.EqualTo(input.AppEnvironmentContext.Environment.RequesterKey), "Should set requester key from environment");
            Assert.That(tempSession.UserAgent, Is.EqualTo(input.AppEnvironmentContext.Environment.UserAgent), "Should set user agent from environment");
            Assert.That(tempSession.RemoteAddress, Is.EqualTo(input.AppEnvironmentContext.Environment.RemoteAddress), "Should set remote address from environment");
        }

        private TestInput setup()
        {
            var services = new ServiceCollection();
            services.AddScoped<TempLog, FakeTempLog>();
            services.AddScoped<TempSessionContext>();
            services.AddScoped<Clock, FakeClock>();
            services.AddScoped<IAppEnvironmentContext>(sp => new FakeAppEnvironmentContext
            {
                Environment = new AppEnvironment("test.user", "my-computer", "10.1.0.0", "Windows 10")
            });
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
            }

            public TempSessionContext TempSessionContext { get; }
            public FakeTempLog TempLog { get; }
            public FakeClock Clock { get; }
            public FakeAppEnvironmentContext AppEnvironmentContext { get; }
        }
    }
}
