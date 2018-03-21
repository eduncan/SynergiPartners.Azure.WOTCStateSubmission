using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SynergiPartners.WOTCScreening.Core
{
    public class VeteranFoodStamps
    {
        [Key]
        public Guid Id { get; set; }
        public bool ReceivedFoodStamps { get; set; }
        public string RecipientFirstName { get; set; }
        public string RecipientLastName { get; set; }
        public string CityReceived { get; set; }
        public string StateReceived { get; set; }
    }
}
