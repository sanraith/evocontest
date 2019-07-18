using System.Threading.Tasks;

namespace evorace.WebApp.Common
{
    public interface IWorkerHubServer
    {
        Task SendMessage(string status);
    }
}
