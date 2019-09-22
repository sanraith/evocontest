using System.Threading.Tasks;

namespace evocontest.WebApp.Common.Hub
{
    public interface IWorkerHubServer
    {
        Task SendMessage(string status);
        Task UpdateStatus(string submissionId, ValidationStateEnum state, string? error);
    }
}
