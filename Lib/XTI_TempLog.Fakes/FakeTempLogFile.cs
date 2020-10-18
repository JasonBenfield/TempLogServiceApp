using System;
using System.Threading.Tasks;

namespace XTI_TempLog.Fakes
{
    public sealed class FakeTempLogFile : ITempLogFile
    {
        private string contents;

        public FakeTempLogFile(string name, DateTime lastModified)
        {
            Name = name;
            LastModified = lastModified;
        }

        public string Name { get; }
        public DateTime LastModified { get; }

        public Task<string> Read() => Task.FromResult(contents);

        public Task Write(string contents)
        {
            this.contents = contents;
            return Task.CompletedTask;
        }

        public void StartProcessing()
        {
        }

        public event EventHandler Deleted;

        public void Delete() => Deleted?.Invoke(this, new EventArgs());

    }
}
