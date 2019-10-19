using System;

namespace evocontest.WebApp.Common
{
    public static class Constants
    {
        public static readonly string LoginRoute = "/Identity/Account/Login";
        public static readonly string WorkerHubRoute = "/workerhub";
        public static readonly string UserHubRoute = "/userhub";
        public static readonly string DownloadSubmissionRoute = "/Worker/DownloadSubmission";
        public static readonly string GetValidSubmissionsRoute = "/Worker/GetValidSubmissions";
        public static readonly string UploadMatchResultsRoute = "/Worker/UploadMatchResults";
        public static readonly string MatchMetadataFileName = "match.json";
        public static readonly DateTime LastSubmissionDate = new DateTime(2019, 10, 21, 10, 0, 0);
    }
}
