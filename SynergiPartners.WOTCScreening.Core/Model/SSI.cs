using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SynergiPartners.WOTCScreening.Core
{
    public class SSI
    {
        [Key]
        public Guid Id { get; set; }
        public bool ReceivedDisabilityPayments { get; set; }
    }
}
