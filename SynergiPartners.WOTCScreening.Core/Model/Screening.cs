using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SynergiPartners.WOTCScreening.Core;

namespace SynergiPartners.WOTCScreening.Core
{
    public class Screening
    {
        [Key]
        public Guid Id { get; set; }
        public Employer Employer { get; set; }
        public Applicant Applicant { get; set; }
        public ConditionalCertification ConditionalCertification { get; set; }
        public FoodStamps FoodStamps { get; set; }
        public Welfare Welfare { get; set; }
        public VocationalRehabilitation VocationalRehabilitation { get; set; }
        public Veteran Veteran { get; set; }
        public SSI SSI { get; set; }
        public Felony Felony { get; set; }
        public LongTermUnemployed LongTermUnemployed { get; set; }
        public List<Document> Documents { get; set; }
        public DateTime? GaveInformation { get; set; }
        public DateTime? OfferedJob { get; set; }
        public DateTime? WasHired { get; set; }
        public DateTime? StartedJob { get; set; }
        public string Position { get; set; }
        public int ONETCode { get; set; }
        public decimal StartingWage { get; set; }
        public string DocumentationSources { get; set; }
        public DateTime? ProcessedDate { get; set; }

        private IRSForm8850 irsForm8850;
        [NotMapped]
        public IRSForm8850 IRSForm8850 => irsForm8850 ?? (irsForm8850 = new IRSForm8850(this));

        private IRSForm9061 irsForm9061;
        [NotMapped]
        public IRSForm9061 IRSForm9061 => irsForm9061 ?? (irsForm9061 = new IRSForm9061(this));

        private TargetGroupCalculator targetGroups;

        [NotMapped]
        public TargetGroupCalculator TargetGroups =>
            targetGroups ?? (targetGroups = new TargetGroupCalculator(this));


    }
}
