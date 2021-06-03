using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class HolidayMaster : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public string HolidayName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsSite { get; set; }
        public Master.Project Project { get; set; }
    }
}
