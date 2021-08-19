using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class VerificationApprovalTemplateValueAuditDto : BaseDto
    {
        public int VerificationApprovalTemplateValueId { get; set; }
        public string Value { get; set; }
        public string Note { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonName { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedDate{ get; set; }
        public string OldValue { get; set; }
    }

    public class VerificationApprovalAuditDto
    {
        public int Id { get; set; }
        public string Variable { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
        public string Note { get; set; }
        public string User { get; set; }
        public string Role { get; set; }

        public DateTime? CreatedDate { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
