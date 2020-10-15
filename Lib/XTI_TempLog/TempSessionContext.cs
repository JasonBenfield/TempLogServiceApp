using System;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_Core;

namespace XTI_TempLog
{
    public sealed class TempSessionContext
    {
        private readonly TempLog log;
        private readonly IAppEnvironmentContext appEnvironmentContext;
        private readonly Clock clock;

        public TempSessionContext(TempLog log, IAppEnvironmentContext appEnvironmentContext, Clock clock)
        {
            this.log = log;
            this.appEnvironmentContext = appEnvironmentContext;
            this.clock = clock;
        }

        private string sessionKey;

        public Task StartSession()
        {
            var environment = appEnvironmentContext.Value();
            sessionKey = Guid.NewGuid().ToString("N");
            var session = new StartSessionModel
            {
                SessionKey = sessionKey,
                TimeStarted = clock.Now(),
                UserName = environment.UserName,
                UserAgent = environment.UserAgent,
                RemoteAddress = environment.RemoteAddress,
                RequesterKey = environment.RequesterKey
            };
            var serialized = JsonSerializer.Serialize(session);
            log.Write($"session.{sessionKey}.log", serialized);
            return Task.CompletedTask;
        }
    }
}
