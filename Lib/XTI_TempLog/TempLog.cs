using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_TempLog
{
    public abstract class TempLog
    {
        public IEnumerable<ITempLogFile> StartSessionFiles() => Files(FileNames("session.*.log"));
        public IEnumerable<ITempLogFile> StartRequestFiles() => Files(FileNames("request.*.log"));
        public IEnumerable<ITempLogFile> LogEventFiles() => Files(FileNames("event.*.log"));

        private IEnumerable<ITempLogFile> Files(IEnumerable<string> fileNames)
            => fileNames.Select(f => CreateFile(f));

        protected abstract IEnumerable<string> FileNames(string pattern);

        public Task Write(string fileName, string contents) => CreateFile(fileName).Write(contents);

        protected abstract ITempLogFile CreateFile(string name);
    }
}
