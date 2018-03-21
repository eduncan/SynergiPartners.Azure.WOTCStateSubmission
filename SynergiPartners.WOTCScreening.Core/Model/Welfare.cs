using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SynergiPartners.WOTCScreening.Core
{
    public class Welfare
    {
        [Key]
        public Guid Id { get; set; }
        public bool ReceivedWelfare { get; set; }
        public string RecipientFirstName { get; set; }
        public string RecipientLastName { get; set; }
        public string CityReceived { get; set; }
        public string StateReceived { get; set; }
        public bool ReceivedBenefitsPast18Months { get; set; }
        public bool ReceivedBenefitsAtLeast9Months { get; set; }
        public bool ReceivedBenefits18MonthsWithinPast2Years { get; set; }
        public bool MaximizedBenefitsWithinPastTwoYears { get; set; }
    }
}
