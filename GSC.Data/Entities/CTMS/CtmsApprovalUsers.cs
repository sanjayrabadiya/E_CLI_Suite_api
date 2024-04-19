using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
namespace GSC.Data.Entities.CTMS
{
    public class CtmsApprovalUsers : BaseEntity, ICommonAduit
    {
        public int CtmsApprovalRolesId { get; set; }
        public int UserId { get; set; }
        public User Users { get; set; }
        public CtmsApprovalRoles CtmsApprovalRoles { get; set; }
    }
}
