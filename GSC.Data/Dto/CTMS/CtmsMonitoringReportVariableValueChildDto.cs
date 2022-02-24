using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.CTMS
{
   public class CtmsMonitoringReportVariableValueChildDto : BaseDto
    {
        public int CtmsMonitoringReportVariableValueId { get; set; }
        public int VariableValueId { get; set; }
        public string Value { get; set; }
        public VariableValue VariableValue { get; set; }
    }
}
