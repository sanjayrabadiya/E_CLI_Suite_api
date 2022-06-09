using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Common;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Data.Dto.Project.Schedule
{
    public class ScheduleTerminateDetailDto : BaseDto
    {
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectScheduleTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
        public Operator Operator { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        public DataType? DataType { get; set; }
        public List<ProjectDesignVariableValueDropDown> ExtraData { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
    }
}
