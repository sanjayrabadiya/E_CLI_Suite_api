using GSC.Helper;

namespace GSC.Data.Entities.Common
{
    public class UserRecentItem : BaseEntity
    {
        public int UserId { get; set; }
        public int KeyId { get; set; }
        public UserRecent ScreenType { get; set; }

        public string SubjectName { get; set; }
        public string SubjectName1 { get; set; }

        public int? RoleId { get; set; }
    }
}