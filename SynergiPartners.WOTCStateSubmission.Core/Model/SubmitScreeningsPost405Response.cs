using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SynergiPartners.WOTCStateSubmission.Core
{
    public class SubmitScreeningsPost405Response
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ResponseId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
