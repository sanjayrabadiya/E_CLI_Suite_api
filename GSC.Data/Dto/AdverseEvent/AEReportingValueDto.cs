using GSC.Data.Entities.AdverseEvent;
using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.AdverseEvent
{
    public class AEReportingValueDto : BaseDto
    {
        public int AEReportingId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
        public AEReporting AEReporting { get; set; }
    }
}
