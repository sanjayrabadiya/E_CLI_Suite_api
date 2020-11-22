using System;
using GSC.Common.Base;

namespace GSC.Data.Entities.UserMgt
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public DateTime ExpiredOn { get; set; }
    }
}