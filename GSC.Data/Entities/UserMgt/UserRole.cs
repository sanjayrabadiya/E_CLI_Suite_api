using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.UserMgt
{
    public class UserRole : BaseEntity, ICommonAduit
    {
        public int UserRoleId { get; set; }

        [ForeignKey("UserRoleId")] public SecurityRole SecurityRole { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")] 
        public User User { get; set; }
    }
}