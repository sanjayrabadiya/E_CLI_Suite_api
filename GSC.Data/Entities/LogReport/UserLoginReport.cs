using System;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;

namespace GSC.Data.Entities.LogReport
{
    public class UserLoginReport : BaseEntity
    {
        private DateTime _LoginTime;


        private DateTime? _LogoutTime;
        public int? UserId { get; set; }
        public string LoginName { get; set; }

        public DateTime LoginTime
        {
            get => _LoginTime.UtcDate();
            set => _LoginTime = value.UtcDate();
        }

        public string Note { get; set; }

        public DateTime? LogoutTime
        {
            get => _LogoutTime.UtcDate();
            set => _LogoutTime = value?.UtcDate();
        }

        public bool IsSessionOut { get; set; }
        public string IpAddress { get; set; }
        [ForeignKey("UserId")]
        public User user { get; set; }
    }
}