using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace GSC.Data.Entities.CTMS
{
    public class CtmsApprovalRoles : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public TriggerType TriggerType { get; set; }
        public string EmailTemplate { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public int SecurityRoleId { get; set; }
        public Master.Project Project { get; set; }
        [ForeignKey("SiteId")]
        public Master.Project Site { get; set; }
        public SecurityRole SecurityRole { get; set; }
    }
}