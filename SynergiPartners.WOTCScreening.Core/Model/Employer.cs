using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SynergiPartners.WOTCScreening.Core
{
    public class Employer
    {
        [Key]
        public Guid Id { get; set; }
        public string EmployerId { get; set; }
        public string LocationId { get; set; }
        public string Name { get; set; }
        public string TelephoneNumber { get; set; }
        public string EIN { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string County { get; set; }
        public string ZipCode { get; set; }
        public bool ShouldUploadEmployer { get; set; }
    }
}
