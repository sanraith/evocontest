using System.Threading.Tasks;

namespace evocontest.WebApp.Common.Hub
{
    public interface IWorkerHubClient
    {
        Task ReceiveMessage(string message);

        Task ValidateSubmissions(params string[] submissionIds);

        Task RunRace();
    }
}
