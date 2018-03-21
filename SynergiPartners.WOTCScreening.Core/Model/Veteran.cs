using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SynergiPartners.WOTCScreening.Core
{
    public class Veteran
    {
        [Key]
        public Guid Id { get; set; }
        public bool VeteranOfArmedForces { get; set; }
        public bool HasServiceConnectedDisability { get; set; }
        public bool DischargedWithinPastYear { get; set; }
        public bool UnemployedLessThan6Months { get; set; }
        public bool UnemployedForAtLeast6Months { get; set; }
        public VeteranFoodStamps VeteranFoodStamps { get; set; }
    }
}
