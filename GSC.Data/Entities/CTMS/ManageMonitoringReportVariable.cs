using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;

namespace GSC.Data.Entities.CTMS
{
    public class ManageMonitoringReportVariable : BaseEntity, ICommonAduit
    {
        public int ManageMonitoringReportId { get; set; }
        public string Value { get; set; }
        public int VariableId { get; set; }
        public int? CompanyId { get; set; }
        public bool IsNa { get; set; }
        public CtmsCommentStatus? QueryStatus { get; set; }
        public ManageMonitoringReport ManageMonitoringReport { get; set; }
        public Variable Variable { get; set; }
        public List<ManageMonitoringReportVariableChild> Children { get; set; }
        public ICollection<ManageMonitoringReportVariableComment> Comments { get; set; }
    }
}