using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateLockUnlockAudit : BaseEntity
    {
        public int ScreeningEntryId { get; set; }
        public int ProjectId { get; set; }
        public int ProjectDesignId { get; set; }
        public int ProjectDesignTemplateId { get; set; }        
        public int CreatedRoleBy { get; set; }
        public bool IsLocked { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }       
        public int AuditReasonId { get; set; }       
        public string AuditReasonComment { get; set; }
        public Master.Project Project { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public ProjectDesign ProjectDesign { get; set; }
        public ScreeningEntry ScreeningEntry { get; set; }        
    }
}
