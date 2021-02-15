using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.AdverseEvent
{
    public class AdverseEventSettingsDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? ProjectDesignTemplateIdPatient { get; set; }
        public int? ProjectDesignTemplateIdInvestigator { get; set; }
        public int? ProjectDesignVisitIdPatient { get; set; }
        public int? ProjectDesignVisitIdInvestigator { get; set; }
        public int SeveritySeqNo1 { get; set; }
        public int SeveritySeqNo2 { get; set; }
        public int SeveritySeqNo3 { get; set; }
    }
}
