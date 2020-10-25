using System;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Api
{
    public sealed class MoveToPermanentAction : AppAction<EmptyRequest, EmptyActionResult>
    {
        private readonly TempLog tempLog;
        private readonly IPermanentLogClient permanentLogClient;
        private readonly Clock clock;

        public MoveToPermanentAction(TempLog tempLog, IPermanentLogClient permanentLogClient, Clock clock)
        {
            this.tempLog = tempLog;
            this.permanentLogClient = permanentLogClient;
            this.clock = clock;
        }

        public async Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            var modifiedBefore = clock.Now().AddMinutes(-1);
            var startSessionFiles = tempLog.StartSessionFiles(modifiedBefore);
            foreach (var startSessionFile in startSessionFiles)
            {
                await processFile<StartSessionModel>
                (
                    startSessionFile,
                    model => permanentLogClient.StartSession(model)
                );
            }
            var startRequestFiles = tempLog.StartRequestFiles(modifiedBefore);
            foreach (var startRequestFile in startRequestFiles)
            {
                await processFile<StartRequestModel>
                (
                    startRequestFile,
                    model => permanentLogClient.StartRequest(model)
                );
            }
            var endRequestFiles = tempLog.EndRequestFiles(modifiedBefore);
            foreach (var endRequestFile in endRequestFiles)
            {
                await processFile<EndRequestModel>
                (
                    endRequestFile,
                    model => permanentLogClient.EndRequest(model)
                );
            }
            var authSessionFiles = tempLog.AuthSessionFiles(modifiedBefore);
            foreach (var authSessionFile in authSessionFiles)
            {
                await processFile<AuthenticateSessionModel>
                (
                    authSessionFile,
                    model => permanentLogClient.AuthenticateSession(model)
                );
            }
            var endSessionFiles = tempLog.EndSessionFiles(modifiedBefore);
            foreach (var endSessionFile in endSessionFiles)
            {
                await processFile<EndSessionModel>
                (
                    endSessionFile,
                    model => permanentLogClient.EndSession(model)
                );
            }
            var logEventFiles = tempLog.LogEventFiles(modifiedBefore);
            foreach (var logEventFile in logEventFiles)
            {
                await processFile<LogEventModel>
                (
                    logEventFile,
                    model => permanentLogClient.LogEvent(model)
                );
            }
            return new EmptyActionResult();
        }

        private async Task processFile<TModel>(ITempLogFile file, Func<TModel, Task> permanentLogAction)
        {
            file.StartProcessing();
            var content = await file.Read();
            var model = JsonSerializer.Deserialize<TModel>(content);
            await permanentLogAction(model);
            file.Delete();
        }
    }
}
