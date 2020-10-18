using System.Collections.Generic;
using System.Threading.Tasks;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Tests
{
    public sealed class FakePermanentLogClient : IPermanentLogClient
    {
        private readonly List<IStartSessionModel> startSessions = new List<IStartSessionModel>();

        public Task StartSession(IStartSessionModel model)
        {
            startSessions.Add(model);
            return Task.CompletedTask;
        }

        public IStartSessionModel[] StartSessions() => startSessions.ToArray();

        private readonly List<IStartRequestModel> startRequests = new List<IStartRequestModel>();

        public IStartRequestModel[] StartRequests() => startRequests.ToArray();

        public Task StartRequest(IStartRequestModel model)
        {
            startRequests.Add(model);
            return Task.CompletedTask;
        }

        private readonly List<IEndRequestModel> endRequests = new List<IEndRequestModel>();

        public IEndRequestModel[] EndRequests() => endRequests.ToArray();

        public Task EndRequest(IEndRequestModel model)
        {
            endRequests.Add(model);
            return Task.CompletedTask;
        }

        private readonly List<IEndSessionModel> endSessions = new List<IEndSessionModel>();

        public IEndSessionModel[] EndSessions() => endSessions.ToArray();

        public Task EndSession(IEndSessionModel model)
        {
            endSessions.Add(model);
            return Task.CompletedTask;
        }

        private readonly List<IAuthenticateSessionModel> authSessions = new List<IAuthenticateSessionModel>();

        public IAuthenticateSessionModel[] AuthSessions() => authSessions.ToArray();

        public Task AuthenticateSession(IAuthenticateSessionModel model)
        {
            authSessions.Add(model);
            return Task.CompletedTask;
        }

        private readonly List<ILogEventModel> logEvents = new List<ILogEventModel>();

        public ILogEventModel[] LogEvents() => logEvents.ToArray();

        public Task LogEvent(ILogEventModel model)
        {
            logEvents.Add(model);
            return Task.CompletedTask;
        }
    }
}
