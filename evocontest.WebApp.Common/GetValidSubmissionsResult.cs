using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace evocontest.WebApp.Common
{
    public sealed class GetValidSubmissionsResult
    {
        [JsonPropertyName(nameof(Submissions))]
        public List<Submission> Submissions { get; set; } = new List<Submission>();

        public sealed class Submission
        {
            [JsonPropertyName(nameof(Id))]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName(nameof(UserName))]
            public string UserName { get; set; } = string.Empty;

            [JsonPropertyName(nameof(FileName))]
            public string FileName { get; set; } = string.Empty;

            [JsonPropertyName(nameof(OriginalFileName))]
            public string OriginalFileName { get; set; } = string.Empty;

            [JsonPropertyName(nameof(IsAdmin))]
            public bool IsAdmin { get; set; }

            [JsonPropertyName(nameof(UploadDate))]
            public DateTime UploadDate { get; set; }
        }
    }
}
