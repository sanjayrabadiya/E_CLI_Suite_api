using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Project.Schedule
{
    public class ScheduleTerminateDetailsDto : BaseDto
    {
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectScheduleTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
        public Operator Operator { get; set; }
    }
}
