using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using SynergiPartners.WOTCScreening.Core;
using SynergiPartners.WOTCStateSubmission.Core;

namespace SynergiPartners.WOTCStateSubmission.Core
{
    public class PrepareScreeningsForStatePost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Type
        {
            get { return "PrepareScreeningsForStatePost"; }
            set { }
        }

        public List<Screening> Screenings { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string State { get; set; }
    }
}
