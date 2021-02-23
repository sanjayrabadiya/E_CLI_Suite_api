using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.AdverseEvent
{
    public class AEReportingValue : BaseEntity, ICommonAduit
    {
        public int AEReportingId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
    }
}
