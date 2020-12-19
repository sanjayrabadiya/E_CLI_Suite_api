using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.UserMgt
{
    public class AppScreenPatient : BaseEntity, ICommonAduit
    {
        public string ScreenCode { get; set; }
        public string ScreenName { get; set; }
        public int SeqNo { get; set; }
    }
}
