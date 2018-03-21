using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SynergiPartners.WOTCScreening.Core
{
    public class ConditionalCertification
    {
        [Key]
        public Guid Id { get; set; }
        public bool ReceivedConditionalCertification { get; set; }
    }
}
