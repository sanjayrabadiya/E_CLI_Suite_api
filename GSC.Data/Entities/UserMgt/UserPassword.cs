using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.UserMgt
{
    public class UserPassword : BaseEntity, ICommonAduit
    {
        public int UserId { get; set; }

        public string Password { get; set; }

        public string Salt { get; set; }
    }
}