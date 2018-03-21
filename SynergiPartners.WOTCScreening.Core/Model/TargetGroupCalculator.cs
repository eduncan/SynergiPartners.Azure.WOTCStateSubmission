using System;
using System.Collections.Generic;
using System.Text;

namespace SynergiPartners.WOTCScreening.Core
{
    public class TargetGroupCalculator
    {
        private Screening screening;

        public TargetGroupCalculator(Screening screening)
        {
            this.screening = screening;
        }

        public bool VeteranFoodStamps
        {
            get { return screening.IRSForm9061.Q13b; }
        }

        public bool DisabledVeteran
        {
            get { return screening.IRSForm9061.Q13d; }
        }

        public bool DisabledVeteran6M
        {
            get { return screening.IRSForm9061.Q13e; }
        }

        public bool FoodStamps
        {
            get { return screening.IRSForm9061.Q14a || screening.IRSForm9061.Q14b; }
        }

        public bool VocationalRehabReferral
        {
            get { return screening.IRSForm9061.Q15a || screening.IRSForm9061.Q15b || screening.IRSForm9061.Q15c; }
        }

        public bool LTFARecipient
        {
            get
            {
                return screening.IRSForm9061.Q16a || screening.IRSForm9061.Q16b || screening.IRSForm9061.Q16c ||
                       screening.IRSForm9061.Q16d;
            }
        }

        public bool Felon
        {
            get { return screening.IRSForm9061.Q17; }
        }

        public bool RuralRenewalCommunity
        {
            get { return screening.IRSForm9061.Q18; }
        }

        public bool SummerYouth
        {
            get { return screening.IRSForm9061.Q19; }
        }

        public bool SSI
        {
            get { return screening.IRSForm9061.Q20; }
        }

        public bool UnemployedVeteran4W
        {
            get { return screening.IRSForm9061.Q21; }
        }

        public bool UnemployedVeteran6M
        {
            get { return screening.IRSForm9061.Q22; }
        }

        public bool LongTermUnemployed
        {
            get { return screening.IRSForm9061.Q23a && screening.IRSForm9061.Q23b; }
        }
    }
}
