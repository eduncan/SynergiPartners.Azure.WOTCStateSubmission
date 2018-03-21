using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace SynergiPartners.WOTCStateSubmission.Core
{
    public class StateSubmissionField
    {
        public int Sequence { get; set; }
        public string Description { get; set; }
        public string Format { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }

        [NotMapped]
        public List<string> Values { get; set; }

        public string ValuesStore
        {
            get { return String.Join("~", Values); }
            set { Values = value != null ? value.Split('~').ToList() : new List<string>(); }
        }
        public string OnGetValueJavascript { get; set; }
    }
}
