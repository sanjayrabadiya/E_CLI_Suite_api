using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Attendance;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.AdverseEvent
{
    public class AEReporting : BaseEntity, ICommonAduit
    {
        public int RandomizationId { get; set; }
       
        public ICollection<AEReportingValue> AEReportingValueValues { get; set; }
        public bool IsReviewedDone { get; set; }
        public int? ReviewedByUser { get; set; }
        public int? ReviewedByRole { get; set; }
       
        public Randomization Randomization { get; set; }
        public bool? IsApproved { get; set; }
        public int? RejectReasonId { get; set; }
        public string RejectReasonOth { get; set; }
        public DateTime? ApproveRejectDateTime { get; set; }
       
        public int? ScreeningTemplateId { get; set; }
        public int? AdverseEventSettingsId { get; set; }
    }
}
