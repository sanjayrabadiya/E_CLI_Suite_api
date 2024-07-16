using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace GSC.Data.Dto.CTMS
{
    public class CtmsApprovalRolesDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int SecurityRoleId { get; set; }
        public string EmailTemplate { get; set; }
        public TriggerType TriggerType { get; set; }

        [NotMapped]
        public IList<CtmsApprovalUsers> CtmsApprovalUsers { get; set; } = null;
        public IList<int> UserIds { get; set; }
    }

    public class CtmsApprovalRolesGridDto : BaseAuditDto
    {
        public string ProjectCode { get; set; }
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public string RoleName { get; set; }
        public string Users { get; set; }
        public int SecurityRoleId { get; set; }
        public string EmailTemplate { get; set; }
        public string TriggerTypeName { get; set; }
        public TriggerType TriggerType { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public int CtmsApprovalRolesId { get; set; }
        public string SiteName { get; set; }
    }

    public class CtmsApprovalUsersGridDto : BaseAuditDto
    {
        public int CtmsApprovalRolesId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ProjectCode { get; set; }
        public string RoleName { get; set; }
        public string TriggerTypeName { get; set; }
    }
}
