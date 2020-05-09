using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Common
{
    public class UserRecentItemDto : BaseDto
    {
        public string UserId { get; set; }
        public int KeyId { get; set; }
        public UserRecent ScreenType { get; set; }

        [Required(ErrorMessage = "Subject Name is required.")]
        public string SubjectName { get; set; }

        public string SubjectName1 { get; set; }
        public string ScreenName { get; set; }
        public int? RoleId { get; set; }
        public ScreenModal ScreenModal { get; set; }
    }

    public class ScreenModal
    {
        public string UrlName { get; set; }
        public string IconPath { get; set; }
    }
}