using System.Threading.Tasks;

namespace XTI_TempLog.Fakes
{
    public sealed class FakeTempLogFile : ITempLogFile
    {
        private string contents;

        public FakeTempLogFile(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Task<string> Read() => Task.FromResult(contents);

        public Task Write(string contents)
        {
            this.contents = contents;
            return Task.CompletedTask;
        }
    }
}
