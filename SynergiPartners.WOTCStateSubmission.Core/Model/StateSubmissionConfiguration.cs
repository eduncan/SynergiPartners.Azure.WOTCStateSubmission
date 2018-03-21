using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using SynergiPartners.WOTCStateSubmission.Core;

namespace SynergiPartners.WOTCStateSubmission.Core
{
    public class StateSubmissionConfiguration
    {
        [Key]
        public Guid Id { get; set; }

        public string Type
        {
            get { return "StateSubmissionConfiguration"; }
            private set { }
        }
        public string State { get; set; }
        public string System { get; set; }
        

        [NotMapped]
        public Dictionary<string, string> AdditionalSystemSettings { get; set; }

        public string AdditionalSystemSettingsString
        {
            get
            {
                string output = string.Empty;
                if (AdditionalSystemSettings != null)
                {
                    bool first = true;
                    foreach (var setting in AdditionalSystemSettings)
                    {
                        if (!first)
                            output += "|";
                        output += string.Format("{0}~{1}", setting.Key, setting.Value);
                        first = false;
                    }
                }

                return output;
            }
            set
            {
                AdditionalSystemSettings = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(value))
                {
                    var settings = value.Split('|');
                    foreach (var setting in settings)
                    {
                        var values = setting.Split('~');

                        AdditionalSystemSettings.Add(values[0], values[1]);
                    }
                }
            }
        }
        public bool EncloseFieldsInQuotes { get; set; }
        public string DefaultDelimiter { get; set; }
        public string OnExecuteJavascript { get; set; }

        public List<StateSubmissionField> HeaderConfiguration { get; set; }
        public List<StateSubmissionField> RecordConfiguration { get; set; }
    }
}
