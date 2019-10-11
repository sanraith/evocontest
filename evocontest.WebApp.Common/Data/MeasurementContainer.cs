using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace evocontest.WebApp.Common.Data
{
    public class MeasurementContainer
    {
        public string SubmissionId { get; set; }

        public List<MeasurementRoundContainer> Rounds { get; set; } = new List<MeasurementRoundContainer>();

        [JsonIgnore]
        public MeasurementRoundContainer? Result => Rounds.OrderByDescending(x => x.DifficultyLevel).FirstOrDefault(x => x.Error == null && x.TotalMilliseconds <= 5000);
    }
}
