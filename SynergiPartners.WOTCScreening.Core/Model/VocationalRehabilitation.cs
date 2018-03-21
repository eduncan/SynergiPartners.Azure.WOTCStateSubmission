using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SynergiPartners.WOTCScreening.Core
{
    public class VocationalRehabilitation
    {
        [Key]
        public Guid Id { get; set; }
        public bool CompletedVocationalRehabilitation { get; set; }
        public bool DoneUnderTicketToWork { get; set; }
        public bool DoneByDepartmentOfVeteransAffairs { get; set; }
    }
}
