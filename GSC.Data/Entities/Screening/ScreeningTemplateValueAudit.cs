using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateValueAudit : BaseEntity
    {
        public int ScreeningTemplateValueId { get; set; }
        public string Value { get; set; }
        public string Note { get; set; }
        public int? ReasonId { get; set; }
        public string OldValue { get; set; }
        public string IpAddress { get; set; }
        public int? UserId { get; set; }
        public int? UserRoleId { get; set; }
        public string TimeZone { get; set; }

        [ForeignKey("ReasonId")] public AuditReason AuditReason { get; set; }
    }
}