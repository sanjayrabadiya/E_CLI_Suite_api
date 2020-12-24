using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Configuration
{
    public class SMSSettingDto : BaseDto
    {
        public string KeyName { get; set; }
        public string SMSurl { get; set; }
        public string SenderId { get; set; }
        public string AuthKey { get; set; }
    }
}
