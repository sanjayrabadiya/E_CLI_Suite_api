using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;

namespace GSC.Data.Entities.Project.Schedule
{
    public class ScheduleTerminateDetail : BaseEntity, ICommonAduit
    {
        public int ProjectScheduleTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
        public Operator Operator { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public ProjectScheduleTemplate ProjectScheduleTemplate { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
    }
}
