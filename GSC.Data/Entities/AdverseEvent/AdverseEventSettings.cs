using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.AdverseEvent
{
    public class AdverseEventSettings : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int? ProjectDesignTemplateIdPatient { get; set; }
        public int? ProjectDesignTemplateIdInvestigator { get; set; }
    }
}
