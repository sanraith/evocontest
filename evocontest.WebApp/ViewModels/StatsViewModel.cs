using evocontest.WebApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace evocontest.WebApp.ViewModels
{
    public class StatsViewModel
    {
        public IReadOnlyList<Measurement> Measurements = new List<Measurement>();

        public StatsViewModel(List<Measurement> measurements)
        {
            Measurements = measurements;
        }
    }
}
