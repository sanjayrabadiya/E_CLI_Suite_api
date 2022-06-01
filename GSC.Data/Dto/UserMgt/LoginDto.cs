using GSC.Data.Dto.Master;
using GSC.Shared.Security;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.UserMgt
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Please provide username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please provide password")]
        public string Password { get; set; }

        public bool IsAnotherDevice { get; set; }

        public int RoleId { get; set; }

        public IList<DropDownDto> Roles { get; set; }
        public bool AskToSelectRole { get; set; }
        public bool IsFirstTime { get; set; }

        public bool IsSuperAdmin { get; set; }

        public UserViewModel CentralUserData { get; set; }
    }
}