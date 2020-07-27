using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateValueQuery : BaseEntity
    {
        public int ScreeningTemplateValueId { get; set; }
        public string Value { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public ScreeningTemplateValue ScreeningTemplateValue { get; set; }

        [ForeignKey("ReasonId")] public AuditReason Reason { get; set; }

        public QueryStatus? QueryStatus { get; set; }
        public short QueryLevel { get; set; }
        public string Note { get; set; }

        [ForeignKey("CreatedBy")] public User CreatedByUser { get; set; }

        public string OldValue { get; set; }
        public int? UserRoleId { get; set; }
        public bool IsSystem { get; set; }
        public string EditCheckRefValue { get; set; }
    }
}