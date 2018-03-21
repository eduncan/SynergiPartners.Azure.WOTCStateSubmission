using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SynergiPartners.WOTCStateSubmission.Core
{
    public class SystemConfiguration
    {
        [Key]
        public Guid Id { get; set; }
        public string SystemName { get; set; }
        public string SystemEndpoint { get; set; }

        [NotMapped]
        public Dictionary<string,string> AdditionalSettings { get; set; }

        public string AdditionalSettingsString
        {
            get
            {
                string output = string.Empty;
                if (AdditionalSettings != null)
                {
                    bool first = true;
                    foreach (var setting in AdditionalSettings)
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
                AdditionalSettings = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(value))
                {
                    var settings = value.Split('|');
                    foreach (var setting in settings)
                    {
                        var values = setting.Split('~');

                        AdditionalSettings.Add(values[0], values[1]);
                    }
                }
            }
        }
    }
}
