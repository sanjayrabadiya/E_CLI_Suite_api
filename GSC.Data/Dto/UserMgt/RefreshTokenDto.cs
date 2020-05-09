using System;

namespace GSC.Data.Dto.UserMgt
{
    public class RefreshTokenDto
    {
        public string AccessToken { get; set; }
        public DateTime? ExpiredAfter { get; set; }
        public string RefreshToken { get; set; }
    }
}