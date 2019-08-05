using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace evorace.WebApp.Common
{
    public sealed class GetValidSubmissionsResult
    {
        [JsonPropertyName(nameof(Submissions))]
        public List<Submission> Submissions { get; set; } = new List<Submission>();

        public sealed class Submission
        {
            [JsonPropertyName(nameof(Id))]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName(nameof(IsValid))]
            public bool? IsValid { get; set; }

            [JsonPropertyName(nameof(UploadDate))]
            public DateTime UploadDate { get; set; }
        }
    }
}
