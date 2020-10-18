using SessionLogWebApp.Api;
using System.Threading.Tasks;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Tests
{
    public sealed class PermanentLogClient : IPermanentLogClient
    {
        private readonly SessionLogApi sessionLogApi;

        public PermanentLogClient(SessionLogApi sessionLogApi)
        {
            this.sessionLogApi = sessionLogApi;
        }

        public Task StartSession(IStartSessionModel model)
            => sessionLogApi.PermanentLog.StartSession.Execute((StartSessionModel)model);

        public Task StartRequest(IStartRequestModel model)
            => sessionLogApi.PermanentLog.StartRequest.Execute((StartRequestModel)model);

        public Task EndRequest(IEndRequestModel model)
            => sessionLogApi.PermanentLog.EndRequest.Execute((EndRequestModel)model);

        public Task EndSession(IEndSessionModel model)
            => sessionLogApi.PermanentLog.EndSession.Execute((EndSessionModel)model);

        public Task AuthenticateSession(IAuthenticateSessionModel model)
            => sessionLogApi.PermanentLog.AuthenticateSession.Execute((AuthenticateSessionModel)model);

        public Task LogEvent(ILogEventModel model)
            => sessionLogApi.PermanentLog.LogEvent.Execute((LogEventModel)model);
    }
}
