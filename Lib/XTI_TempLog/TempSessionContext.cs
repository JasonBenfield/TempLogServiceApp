using System;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_Core;

namespace XTI_TempLog
{
    public sealed class TempSessionContext
    {
        private readonly CurrentSession currentSession;
        private readonly TempLog log;
        private readonly IAppEnvironmentContext appEnvironmentContext;
        private readonly Clock clock;

        public TempSessionContext(CurrentSession currentSession, TempLog log, IAppEnvironmentContext appEnvironmentContext, Clock clock)
        {
            this.currentSession = currentSession;
            this.log = log;
            this.appEnvironmentContext = appEnvironmentContext;
            this.clock = clock;
        }

        public Task StartSession()
        {
            var environment = appEnvironmentContext.Value();
            currentSession.SessionKey = Guid.NewGuid().ToString("N");
            var session = new StartSessionModel
            {
                SessionKey = currentSession.SessionKey,
                TimeStarted = clock.Now(),
                UserName = environment.UserName,
                UserAgent = environment.UserAgent,
                RemoteAddress = environment.RemoteAddress,
                RequesterKey = environment.RequesterKey
            };
            var serialized = JsonSerializer.Serialize(session);
            return log.Write($"session.{session.SessionKey}.log", serialized);
        }

        public Task StartRequest(string path)
        {
            var request = new StartRequestModel
            {
                RequestKey = Guid.NewGuid().ToString("N"),
                SessionKey = currentSession.SessionKey,
                VersionKey = appEnvironmentContext.Value().VersionKey,
                Path = path,
                TimeStarted = clock.Now()
            };
            var serialized = JsonSerializer.Serialize(request);
            return log.Write($"request.{request.RequestKey}.log", serialized);
        }
    }
}
