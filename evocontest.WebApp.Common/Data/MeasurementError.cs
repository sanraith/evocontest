using System;
using System.Collections.Generic;
using System.Text;

namespace evocontest.WebApp.Common.Data
{
    public class MeasurementError
    {
        public MeasurementErrorType ErrorType { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
