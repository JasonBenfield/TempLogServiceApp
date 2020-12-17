using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<ITempLogFile> filesInProgress = new List<ITempLogFile>();

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
            filesInProgress.Clear();
            var logBatch = await processBatch(logs, modifiedBefore);
            while (hasAnyToProcess(logBatch))
            {
                await permanentLogClient.LogBatch(logBatch);
                deleteFiles(filesInProgress);
                filesInProgress.Clear();
                logBatch = await processBatch(logs, modifiedBefore);
            }
            return new EmptyActionResult();
        }

        private static bool hasAnyToProcess(LogBatchModel logBatch)
        {
            return logBatch.StartSessions.Any()
                || logBatch.StartRequests.Any()
                || logBatch.AuthenticateSessions.Any()
                || logBatch.LogEvents.Any()
                || logBatch.EndRequests.Any()
                || logBatch.EndSessions.Any();
        }

        private async Task<LogBatchModel> processBatch(IEnumerable<TempLog> logs, DateTime modifiedBefore)
        {
            var logBatch = new LogBatchModel();
            var startSessionFiles = startProcessingFiles(logs, l => l.StartSessionFiles(modifiedBefore));
            logBatch.StartSessions = await deserializeFiles<StartSessionModel>(startSessionFiles);
            var startRequestFiles = startProcessingFiles(logs, l => l.StartRequestFiles(modifiedBefore));
            logBatch.StartRequests = await deserializeFiles<StartRequestModel>(startRequestFiles);
            var authSessionFiles = startProcessingFiles(logs, l => l.AuthSessionFiles(modifiedBefore));
            logBatch.AuthenticateSessions = await deserializeFiles<AuthenticateSessionModel>(authSessionFiles);
            var logEventFiles = startProcessingFiles(logs, l => l.LogEventFiles(modifiedBefore));
            logBatch.LogEvents = await deserializeFiles<LogEventModel>(logEventFiles);
            var endRequestFiles = startProcessingFiles(logs, l => l.EndRequestFiles(modifiedBefore));
            logBatch.EndRequests = await deserializeFiles<EndRequestModel>(endRequestFiles);
            var endSessionFiles = startProcessingFiles(logs, l => l.EndSessionFiles(modifiedBefore));
            logBatch.EndSessions = await deserializeFiles<EndSessionModel>(endSessionFiles);
            return logBatch;
        }

        private IEnumerable<ITempLogFile> startProcessingFiles(IEnumerable<TempLog> logs, Func<TempLog, IEnumerable<ITempLogFile>> getFiles)
        {
            var filesToProcess = logs
                .SelectMany(l => getFiles(l))
                .Take(50)
                .Select(f => f.WithNewName($"{f.Name}.processing"))
                .ToArray();
            filesInProgress.AddRange(filesToProcess);
            return filesToProcess;
        }

        private async Task<T[]> deserializeFiles<T>(IEnumerable<ITempLogFile> files)
        {
            var deserialized = new List<T>();
            foreach (var file in files)
            {
                var content = await file.Read();
                var model = JsonSerializer.Deserialize<T>(content);
                deserialized.Add(model);
            }
            return deserialized.ToArray();
        }

        private void deleteFiles(IEnumerable<ITempLogFile> files)
        {
            foreach (var file in files)
            {
                file.Delete();
            }
        }
    }
}
