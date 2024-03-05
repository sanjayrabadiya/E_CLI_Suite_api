using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.StudyLevelFormSetup;

namespace GSC.Data.Dto.CTMS
{
    public class CtmsMonitoringReportVariableValueChildDto : BaseDto
    {
        public int CtmsMonitoringReportVariableValueId { get; set; }
        public int StudyLevelFormVariableValueId { get; set; }
        public string Value { get; set; }
        public StudyLevelFormVariableValue StudyLevelFormVariableValue { get; set; }
        public short? LevelNo { get; set; }
    }
}
