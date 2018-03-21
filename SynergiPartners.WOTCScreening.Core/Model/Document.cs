using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SynergiPartners.WOTCScreening.Core
{
    public class Document
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public bool SignatureRequired { get; set; }
        public string SignedName { get; set; }
        public string SignedIpAddress { get; set; }
        public DateTime? SignedDate { get; set; }
    }
}
