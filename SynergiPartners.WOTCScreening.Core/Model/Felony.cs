using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SynergiPartners.WOTCScreening.Core
{
    public class Felony
    {
        [Key]
        public Guid Id { get; set; }
        public bool HasFelonyConviction { get; set; }
        public DateTime? ConvictionDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public bool FederalConviction { get; set; }
    }
}
