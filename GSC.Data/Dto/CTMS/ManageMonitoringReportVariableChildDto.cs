using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.CTMS
{
   public class ManageMonitoringReportVariableChildDto : BaseDto
    {
        public int ManageMonitoringReportVariableId { get; set; }
        public int VariableValueId { get; set; }
        public string Value { get; set; }
        public VariableValue VariableValue { get; set; }
    }
}
