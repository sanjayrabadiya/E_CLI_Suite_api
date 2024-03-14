using GSC.Common.Base;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateLockUnlockAudit : BaseEntity
    {
        public int ScreeningEntryId { get; set; }
        public int ProjectId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int CreatedRoleBy { get; set; }
        public bool IsLocked { get; set; }
        public bool IsHardLocked { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public int AuditReasonId { get; set; }
        public string AuditReasonComment { get; set; }
        public Master.Project Project { get; set; }
        public ScreeningTemplate ScreeningTemplate { get; set; }
        public ScreeningEntry ScreeningEntry { get; set; }
        public string DataEntryStatus { get; set; }
    }
}
