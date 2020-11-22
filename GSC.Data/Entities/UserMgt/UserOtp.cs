using GSC.Common.Base;

namespace GSC.Data.Entities.UserMgt
{
    public class UserOtp : BaseEntity
    {
        public int UserId { get; set; }

        public string Otp { get; set; }
    }
}