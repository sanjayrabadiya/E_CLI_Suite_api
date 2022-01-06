using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.LabManagement
{
    public class LabManagementSendEmailUser : BaseEntity, ICommonAduit
    {
        public int LabManagementConfigurationId { get; set; }
        public int UserId { get; set; }
        public LabManagementConfiguration LabManagementConfiguration { get; set; }
        public User User { get; set; }
    }
}
