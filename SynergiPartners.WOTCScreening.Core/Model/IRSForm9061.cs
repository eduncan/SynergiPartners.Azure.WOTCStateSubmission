using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using SynergiPartners.WOTCScreening.Core;

namespace SynergiPartners.WOTCScreening.Core
{
    public class IRSForm9061
    {
        private Screening screening;

        public IRSForm9061(Screening screening)
        {
            this.screening = screening;
        }

        public bool Q12
        {
            get
            {
                if (screening.Applicant != null)
                {
                    var dateOfBirth = screening.Applicant.DateOfBirth ?? DateTime.MinValue;
                    var gaveInformation = screening.GaveInformation ?? DateTime.MinValue;

                    var years = gaveInformation.Year - dateOfBirth.Year;

                    if ((dateOfBirth.Month == gaveInformation.Month &&
                         gaveInformation.Day < dateOfBirth.Day) ||
                        gaveInformation.Month < dateOfBirth.Month)
                    {
                        years--;
                    }

                    return (years >= 16) && (years < 40);
                }

                return false;
            }
        }

        public bool Q13a
        {
            get { return screening.Veteran != null && screening.Veteran.VeteranOfArmedForces; }
        }

        public bool Q13b
        {
            get
            {
                return screening.Veteran != null && (screening.Veteran.VeteranFoodStamps != null &&
                                                     screening.Veteran.VeteranFoodStamps.ReceivedFoodStamps);
            }
        }

        public string Q13bFirstName
        {
            get
            {
                return Q13b ? 
                    screening.Veteran.VeteranFoodStamps.RecipientFirstName
                 : string.Empty;
            }
        }

        public string Q13bLastName
        {
            get
            {
                return Q13b ?
                    screening.Veteran.VeteranFoodStamps.RecipientLastName
                    : string.Empty;
            }
        }

        public string Q13bCity
        {
            get
            {
                return Q13b ?
                    screening.Veteran.VeteranFoodStamps.CityReceived
                    : string.Empty;
            }
        }

        public string Q13bState
        {
            get
            {
                return Q13b ?
                    screening.Veteran.VeteranFoodStamps.StateReceived
                    : string.Empty;
            }
        }

        public bool Q13c
        {
            get { return screening.Veteran != null && screening.Veteran.HasServiceConnectedDisability; }
        }

        public bool Q13d
        {
            get { return screening.Veteran != null && screening.Veteran.DischargedWithinPastYear; }
        }

        public bool Q13e
        {
            get { return screening.Veteran != null && screening.Veteran.UnemployedForAtLeast6Months; }
        }

        public bool Q14a
        {
            get { return screening.FoodStamps != null && screening.FoodStamps.ReceivedFoodStampsLast6Months; }
        }

        public bool Q14b
        {
            get { return screening.FoodStamps != null && screening.FoodStamps.ReceivedFoodStampsButNotNow; }
        }

        public string Q14FirstName
        {
            get { return screening.FoodStamps==null?null : screening.FoodStamps.RecipientFirstName; }
        }

        public string Q14LastName
        {
            get { return screening.FoodStamps==null?null:screening.FoodStamps.RecipientLastName; }
        }

        public string Q14City
        {
            get { return screening.FoodStamps==null?null:screening.FoodStamps.CityReceived; }
        }

        public string Q14State
        {
            get { return screening.FoodStamps==null?null:screening.FoodStamps.StateReceived; }
        }

        public bool Q15a
        {
            get { return screening.VocationalRehabilitation != null && screening.VocationalRehabilitation.CompletedVocationalRehabilitation; }
        }

        public bool Q15b
        {
            get { return screening.VocationalRehabilitation != null && screening.VocationalRehabilitation.DoneUnderTicketToWork; }
        }

        public bool Q15c
        {
            get { return screening.VocationalRehabilitation != null && screening.VocationalRehabilitation.DoneByDepartmentOfVeteransAffairs; }
        }

        public bool Q16a
        {
            get { return screening.Welfare != null && screening.Welfare.ReceivedBenefitsPast18Months; }
        }

        public bool Q16b
        {
            get { return screening.Welfare != null && screening.Welfare.ReceivedBenefits18MonthsWithinPast2Years; }
        }

        public bool Q16c
        {
            get { return screening.Welfare != null && screening.Welfare.MaximizedBenefitsWithinPastTwoYears; }
        }

        public bool Q16d
        {
            get { return screening.Welfare != null && screening.Welfare.ReceivedBenefitsAtLeast9Months; }
        }

        public string Q16FirstName
        {
            get { return screening.Welfare==null?null:screening.Welfare.RecipientFirstName; }
        }

        public string Q16LastName
        {
            get { return screening.Welfare==null?null:screening.Welfare.RecipientLastName; }
        }

        public string Q16City
        {
            get { return screening.Welfare==null?null:screening.Welfare.CityReceived; }
        }

        public string Q16State
        {
            get { return screening.Welfare==null?null:screening.Welfare.StateReceived; }
        }

        public bool Q17
        {
            get { return screening.Felony != null && screening.Felony.HasFelonyConviction; }
        }

        public DateTime? Q17ConvictionDate
        {
            get { return screening.Felony==null?null:screening.Felony.ConvictionDate; }
        }

        public DateTime? Q17ReleaseDate
        {
            get { return screening.Felony==null?null:screening.Felony.ReleaseDate; }
        }

        public bool Q17FederalConviction
        {
            get { return screening.Felony != null && screening.Felony.FederalConviction; }
        }

        public bool Q17StateConviction
        {
            get { return Q17 ? !Q17FederalConviction : false; }
        }

        public bool Q18
        {
            get { return screening.Applicant != null && screening.Applicant.LivesInZone; }
        }

        public bool Q19
        {
            get
            {
                if (screening.Applicant != null && screening.Applicant.LivesInZone)
                {
                    var dateOfBirth = screening.Applicant.DateOfBirth ?? DateTime.MinValue;
                    var gaveInformation = screening.GaveInformation ?? DateTime.MinValue;

                    var years = gaveInformation.Year - dateOfBirth.Year;

                    if ((dateOfBirth.Month == gaveInformation.Month &&
                         gaveInformation.Day < dateOfBirth.Day) ||
                        gaveInformation.Month < dateOfBirth.Month)
                    {
                        years--;
                    }

                    return (years >= 16) && (years < 18);
                }

                return false;
            }
        }

        public bool Q20
        {
            get { return screening.SSI != null && screening.SSI.ReceivedDisabilityPayments; }
        }

        public bool Q21
        {
            get { return screening.Veteran != null && screening.Veteran.UnemployedForAtLeast6Months; }
        }

        public bool Q22
        {
            get { return screening.Veteran != null && screening.Veteran.UnemployedLessThan6Months; }
        }

        public bool Q23a
        {
            get { return screening.LongTermUnemployed != null && screening.LongTermUnemployed.Unemployed6Months; }
        }

        public bool Q23b
        {
            get { return screening.LongTermUnemployed != null && screening.LongTermUnemployed.ReceivedUnemploymentCompensation; }
        }

        public string Q24
        {
            get { return screening.DocumentationSources; }
        }
    }
}
