using System.Threading.Tasks;

namespace evorace.WebApp.Common.Hub
{
    public interface IWorkerHubServer
    {
        Task SendMessage(string status);
        Task UpdateStatus(string submissionId, ValidationStateEnum state, string? error);
    }
}
