using System;
using System.Collections.Generic;
using System.Text;

namespace evocontest.WebApp.Common.Data
{
    public class MatchContainer
    {
        public DateTime? MatchDate { get; set; }

        public List<MeasurementContainer> Measurements { get; set; }
    }
}
