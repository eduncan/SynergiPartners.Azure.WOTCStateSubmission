using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SynergiPartners.WOTCScreening.Core
{
    public class LongTermUnemployed
    {
        [Key]
        public Guid Id { get; set; }
        public bool Unemployed6Months { get; set; }
        public bool ReceivedUnemploymentCompensation { get; set; }
        public string StateReceived { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
