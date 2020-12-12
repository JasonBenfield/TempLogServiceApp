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
        private readonly TempLogs tempLogs;
        private readonly IPermanentLogClient permanentLogClient;
        private readonly Clock clock;

        public MoveToPermanentAction(TempLogs tempLogs, IPermanentLogClient permanentLogClient, Clock clock)
        {
            this.tempLogs = tempLogs;
            this.permanentLogClient = permanentLogClient;
            this.clock = clock;
        }

        public async Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            var modifiedBefore = clock.Now().AddMinutes(-1);
            var logs = tempLogs.Logs();
            foreach (var log in logs)
            {
                var startSessionFiles = log.StartSessionFiles(modifiedBefore);
                foreach (var startSessionFile in startSessionFiles)
                {
                    await processFile<StartSessionModel>
                    (
                        startSessionFile,
                        model => permanentLogClient.StartSession(model)
                    );
                }
                var startRequestFiles = log.StartRequestFiles(modifiedBefore);
                foreach (var startRequestFile in startRequestFiles)
                {
                    await processFile<StartRequestModel>
                    (
                        startRequestFile,
                        model => permanentLogClient.StartRequest(model)
                    );
                }
                var endRequestFiles = log.EndRequestFiles(modifiedBefore);
                foreach (var endRequestFile in endRequestFiles)
                {
                    await processFile<EndRequestModel>
                    (
                        endRequestFile,
                        model => permanentLogClient.EndRequest(model)
                    );
                }
                var authSessionFiles = log.AuthSessionFiles(modifiedBefore);
                foreach (var authSessionFile in authSessionFiles)
                {
                    await processFile<AuthenticateSessionModel>
                    (
                        authSessionFile,
                        model => permanentLogClient.AuthenticateSession(model)
                    );
                }
                var endSessionFiles = log.EndSessionFiles(modifiedBefore);
                foreach (var endSessionFile in endSessionFiles)
                {
                    await processFile<EndSessionModel>
                    (
                        endSessionFile,
                        model => permanentLogClient.EndSession(model)
                    );
                }
                var logEventFiles = log.LogEventFiles(modifiedBefore);
                foreach (var logEventFile in logEventFiles)
                {
                    await processFile<LogEventModel>
                    (
                        logEventFile,
                        model => permanentLogClient.LogEvent(model)
                    );
                }
            }
            return new EmptyActionResult();
        }

        private async Task processFile<TModel>(ITempLogFile file, Func<TModel, Task> permanentLogAction)
        {
            var renamedFile = file.WithNewName($"{file.Name}.processing");
            var content = await renamedFile.Read();
            var model = JsonSerializer.Deserialize<TModel>(content);
            await permanentLogAction(model);
            renamedFile.Delete();
        }
    }
}
