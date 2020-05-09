using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.UserMgt
{
    public class UserRole : BaseEntity
    {
        public int UserRoleId { get; set; }

        [ForeignKey("UserRoleId")] public SecurityRole SecurityRole { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")] public User User { get; set; }
    }
}