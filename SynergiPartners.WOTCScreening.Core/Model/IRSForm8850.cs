using System;
using System.Collections.Generic;
using System.Text;
using SynergiPartners.WOTCScreening.Core;

namespace SynergiPartners.WOTCScreening.Core
{
    public class IRSForm8850
    {
        private Screening screening;

        public IRSForm8850(Screening screening)
        {
            this.screening = screening;
        }

        public bool Q1
        {
            // conditional certification
            get { return screening.ConditionalCertification != null && screening.ConditionalCertification.ReceivedConditionalCertification; }
        }

        public bool Q2
        {
            get
            {
                // unemployed veteran
                if (screening.Veteran != null && screening.Veteran.VeteranOfArmedForces)
                {
                    if (screening.Veteran.UnemployedLessThan6Months)
                    {
                        return true;
                    }
                }


                // TANF
                if (screening.Welfare != null && screening.Welfare.ReceivedWelfare)
                {
                    if (screening.Welfare.ReceivedBenefitsAtLeast9Months)
                    {
                        return true;
                    }
                }

                // vocational rehab
                if (screening.VocationalRehabilitation != null && screening.VocationalRehabilitation.CompletedVocationalRehabilitation)
                {
                    return true;
                }

                // food stamps
                if (screening.FoodStamps != null && screening.FoodStamps.ReceivedFoodStamps)
                {
                    return true;
                }

                // felony
                if (screening.Felony != null && screening.Felony.HasFelonyConviction)
                {
                    return true;
                }

                return false;
            }
        }

        // unemployed veteran 6 mo
        public bool Q3
        {
            get
            {
                if (screening.Veteran != null && screening.Veteran.VeteranOfArmedForces)
                {
                    if (screening.Veteran.UnemployedForAtLeast6Months)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // disabled veteran
        public bool Q4
        {
            get
            {
                if (screening.Veteran != null && screening.Veteran.VeteranOfArmedForces)
                {
                    if (screening.Veteran.HasServiceConnectedDisability)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // disabled and unemployed veteran
        public bool Q5
        {
            get { return Q3 & Q4; }
        }

        public bool Q6
        {
            get
            {
                if (screening.Welfare != null && screening.Welfare.ReceivedWelfare)
                {
                    if (screening.Welfare.ReceivedBenefitsPast18Months)
                    {
                        return true;
                    }

                    if (screening.Welfare.ReceivedBenefits18MonthsWithinPast2Years)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool Q7
        {
            get
            {
                if (screening.LongTermUnemployed != null && screening.LongTermUnemployed.Unemployed6Months)
                {
                    if (screening.LongTermUnemployed.ReceivedUnemploymentCompensation)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
