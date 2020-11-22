using GSC.Common.Base;

namespace GSC.Data.Entities.UserMgt
{
    public class UserFavoriteScreen : BaseEntity
    {
        public int AppScreenId { get; set; }
        public int UserId { get; set; }
    }
}