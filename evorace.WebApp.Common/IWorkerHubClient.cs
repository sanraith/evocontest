using System.Threading.Tasks;

namespace evorace.WebApp.Common
{
    public interface IWorkerHubClient
    {
        Task ReceiveMessage(string message);

        Task ValidateSubmissions(params string[] submissionIds);

        Task RunRace();
    }
}
