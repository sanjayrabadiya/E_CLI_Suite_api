using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Configuration
{
    public class SMSSetting : BaseEntity, ICommonAduit
    {
        public string KeyName { get; set; }
        public string SMSurl { get; set; }
        public string SenderId { get; set; }
        public string AuthKey { get; set; }
    }
}
