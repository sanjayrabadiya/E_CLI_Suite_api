using GSC.Common.Base;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentReviewDetailsAudit: BaseEntity
    {
        public int EconsentReviewDetailsId { get; set; }
        public ICFAction Activity { get; set; }
        public ScreeningPatientStatus PateientStatus { get; set; }

        public EconsentReviewDetails EconsentReviewDetails { get; set; }
    }
}
