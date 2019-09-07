using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace evorace.WebApp.Data
{
    public partial class Match
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string Id { get; set; }

        public virtual List<Measurement> Measurements { get; set; }

        [Required]
        public DateTime MatchDate { get; set; }

        public string JsonResult { get; set; }

        public Match()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
