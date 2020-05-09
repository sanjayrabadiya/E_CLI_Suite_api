using System;
using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.UserMgt
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public DateTime ExpiredOn { get; set; }
    }
}