using System;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.LogReport
{
    public class UserLoginReportDto : BaseDto
    {
        public int? UserId { get; set; }
        public string LoginName { get; set; }
        public DateTime LoginTime { get; set; }

        public DateTime? LogoutTime { get; set; }

        public bool IsSessionOut { get; set; }

        public string Note { get; set; }

        public string IpAddress { get; set; }
    }
}