using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.UserMgt
{
    public class UserOtp : BaseEntity
    {
        public int UserId { get; set; }

        public string Otp { get; set; }
    }
}