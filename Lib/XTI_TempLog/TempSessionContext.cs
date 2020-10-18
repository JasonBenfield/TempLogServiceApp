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
            currentSession.SessionKey = generateKey();
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
            return log.Write($"startSession.{session.SessionKey}.log", serialized);
        }

        public Task AuthenticateSession(string userName)
        {
            var session = new AuthenticateSessionModel
            {
                SessionKey = currentSession.SessionKey,
                UserName = userName
            };
            var serialized = JsonSerializer.Serialize(session);
            return log.Write($"authSession.{session.SessionKey}.log", serialized);
        }

        private string requestKey;

        public Task StartRequest(string path)
        {
            requestKey = generateKey();
            var env = appEnvironmentContext.Value();
            var request = new StartRequestModel
            {
                RequestKey = requestKey,
                SessionKey = currentSession.SessionKey,
                AppKey = env.AppKey,
                VersionKey = env.VersionKey,
                Path = path,
                TimeStarted = clock.Now()
            };
            var serialized = JsonSerializer.Serialize(request);
            return log.Write($"startRequest.{request.RequestKey}.log", serialized);
        }

        private string generateKey() => Guid.NewGuid().ToString("N");

        public Task EndRequest()
        {
            var request = new EndRequestModel
            {
                RequestKey = requestKey,
                TimeEnded = clock.Now()
            };
            var serialized = JsonSerializer.Serialize(request);
            return log.Write($"endRequest.{request.RequestKey}.log", serialized);
        }

        public Task EndSession()
        {
            var request = new EndSessionModel
            {
                SessionKey = currentSession.SessionKey,
                TimeEnded = clock.Now()
            };
            var serialized = JsonSerializer.Serialize(request);
            return log.Write($"endSession.{request.SessionKey}.log", serialized);
        }

        public Task LogException(AppEventSeverity severity, Exception ex, string caption)
        {
            var tempEvent = new LogEventModel
            {
                EventKey = generateKey(),
                RequestKey = requestKey,
                TimeOccurred = clock.Now(),
                Severity = severity.Value,
                Caption = caption,
                Message = ex.Message,
                Detail = ex.StackTrace
            };
            var serialized = JsonSerializer.Serialize(tempEvent);
            return log.Write($"event.{tempEvent.EventKey}.log", serialized);
        }

    }
}
