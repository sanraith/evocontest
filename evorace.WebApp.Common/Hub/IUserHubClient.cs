using System.Threading.Tasks;

namespace evorace.WebApp.Common.Hub
{
    public interface IUserHubClient
    {
        Task UpdateUploadStatus(ValidationStateEnum state, bool? isValid, string error);
    }
}
