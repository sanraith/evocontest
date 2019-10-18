using evocontest.WebApp.Data;
using System.Collections.Generic;

namespace evocontest.WebApp.ViewModels
{
    public class RankingsViewModel
    {
        public IList<ApplicationUser> AdminUsers { get; set;}

        public List<Match> OrderedMatches { get; set; }

        public Match LastMatch { get; set; }

        public List<Measurement> LastMatchOrderedMeasurements { get; set; }

        public List<Measurement> LastMatchInvalidMeasurements { get; set; }

        public Measurement? AdminMatch { get; set; }
    }
}
