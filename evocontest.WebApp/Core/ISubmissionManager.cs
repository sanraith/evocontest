using evocontest.WebApp.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace evocontest.WebApp.Core
{
    public interface ISubmissionManager
    {
        Task<(Stream?, string?)> DownloadSubmission(string submissionId);
        Task<GetValidSubmissionsResult> GetValidSubmissions();
    }
}
