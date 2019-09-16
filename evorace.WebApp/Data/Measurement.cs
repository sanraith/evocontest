using evorace.WebApp.Common.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace evorace.WebApp.Data
{
    public partial class Measurement
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string Id { get; set; }

        [Required]
        public virtual Match Match { get; set; }

        [Required]
        public virtual Submission Submission { get; set; }

        public string JsonResult { get; set; }

        public Measurement()
        {
            Id = Guid.NewGuid().ToString();
        }

        [NotMapped]
        public MeasurementContainer MeasurementResult => myMeasurementResult ?? (myMeasurementResult = JsonSerializer.Deserialize<MeasurementContainer>(JsonResult));

        [NotMapped]
        private MeasurementContainer myMeasurementResult;
    }
}
