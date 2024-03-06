using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.StudyLevelFormSetup;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsMonitoringReportVariableValueChild : BaseEntity, ICommonAduit
    {
        public int CtmsMonitoringReportVariableValueId { get; set; }
        public int StudyLevelFormVariableValueId { get; set; }
        public string Value { get; set; }
        public CtmsMonitoringReportVariableValue CtmsMonitoringReportVariableValue { get; set; }
        public StudyLevelFormVariableValue StudyLevelFormVariableValue { get; set; }
        public short? LevelNo { get; set; }
    }
}
