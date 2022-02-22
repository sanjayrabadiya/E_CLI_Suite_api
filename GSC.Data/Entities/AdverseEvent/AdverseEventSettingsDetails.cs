using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.AdverseEvent
{
    public class AdverseEventSettingsDetails : BaseEntity, ICommonAduit
    {
        public int AdverseEventSettingsId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int ProjectDesignVariableValueId { get; set; }
        public string Severity { get; set; }
        public string Value { get; set; }
        public int SeveritySeqNo { get; set; }

    }
}
