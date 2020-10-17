using System.Threading.Tasks;

namespace XTI_TempLog.Abstractions
{
    public interface IPermanentLogClient
    {
        Task StartSession(IStartSessionModel model);
    }
}
