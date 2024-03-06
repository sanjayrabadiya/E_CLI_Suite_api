
using GSC.Common.Base;
using GSC.Data.Entities.UserMgt;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementEmailConfigurationDetail : BaseEntity
    {
        public int SupplyManagementEmailConfigurationId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public SupplyManagementEmailConfiguration SupplyManagementEmailConfiguration { get; set; }
        [ForeignKey("RoleId")]
        public SecurityRole SecurityRole { get; set; }
        public User Users { get; set; }


    }
}
