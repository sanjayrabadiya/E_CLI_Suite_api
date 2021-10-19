using GSC.Common.Base;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class ManageMonitoringReportVariableChild : BaseEntity
    {
        public int ManageMonitoringReportVariableId { get; set; }
        public int VariableValueId { get; set; }
        public string Value { get; set; }
        public VariableValue VariableValue { get; set; }
    }
}
