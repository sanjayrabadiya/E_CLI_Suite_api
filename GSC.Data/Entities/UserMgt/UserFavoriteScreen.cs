using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.UserMgt
{
    public class UserFavoriteScreen : BaseEntity
    {
        public int AppScreenId { get; set; }
        public int UserId { get; set; }
    }
}