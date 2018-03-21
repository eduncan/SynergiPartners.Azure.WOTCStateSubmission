using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using SynergiPartners.WOTCStateSubmission.Core;

namespace SynergiPartners.WOTCStateSubmission.Core
{
    public class UpdateStateSubmissionConfigurationPost
    {
        public StateSubmissionConfiguration StateSubmissionConfiguration { get; set; }
    }
}
