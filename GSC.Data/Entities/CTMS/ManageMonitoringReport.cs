using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Shared.Extension;
using System;

namespace GSC.Data.Entities.CTMS
{
    public class ManageMonitoringReport : BaseEntity, ICommonAduit
    {
        public int ManageMonitoringVisitId { get; set; }
        public short Status { get; set; }
        public int VariableTemplateId { get; set; }
        public int? CompanyId { get; set; }
        public ManageMonitoringVisit ManageMonitoringVisit { get; set; }
        public VariableTemplate VariableTemplate { get; set; }
    }
}