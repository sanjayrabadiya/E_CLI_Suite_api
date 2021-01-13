using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.AdverseEvent
{
    public class AEReporting : BaseEntity, ICommonAduit
    {
        public int RandomizationId { get; set; }
        public string EventDescription { get; set; }
        public DateTime StartDate { get; set; }
        public AdverseEventEffect EventEffect { get; set; }
        public bool IsReviewedDone { get; set; }
        public int? ReviewedByUser { get; set; }
        public int? ReviewedByRole { get; set; }
        public DateTime? ReviewedDateTime { get; set; }
    }
}
