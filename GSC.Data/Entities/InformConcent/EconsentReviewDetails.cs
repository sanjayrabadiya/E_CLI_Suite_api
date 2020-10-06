using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentReviewDetails : BaseEntity
    {
        public int Id { get; set; }
        public int AttendanceId { get; set; }
        public int EconsentDocumentId { get; set; }
        public bool IsApprovedByInvestigator { get; set; }
        public int ApprovedByRoleId { get; set; }
        public DateTime? investigatorapproveddatetime { get; set; }
        public DateTime? patientapproveddatetime { get; set; }
        public string? patientdigitalSignImagepath { get; set; }
        public string? investigatordigitalSignImagepath { get; set; }


    }
}
