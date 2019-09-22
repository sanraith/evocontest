using System.Threading.Tasks;

namespace evocontest.WebApp.Common.Hub
{
    public interface IUserHubClient
    {
        Task UpdateUploadStatus(ValidationStateEnum state, bool? isValid, string error);
    }
}
