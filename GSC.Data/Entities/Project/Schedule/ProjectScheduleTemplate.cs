using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;

namespace GSC.Data.Entities.Project.Schedule
{
    public class ProjectScheduleTemplate : BaseEntity
    {
        public int ProjectScheduleId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }

        public int PositiveDeviation { get; set; }
        public int NegativeDeviation { get; set; }

        [ForeignKey("ProjectDesignTemplateId")]
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }

        [ForeignKey("ProjectDesignVariableId")]
        public ProjectDesignVariable ProjectDesignVariable { get; set; }

        public int ProjectDesignPeriodId { get; set; }
        public int ProjectDesignVisitId { get; set; }

        [ForeignKey("ProjectDesignPeriodId")] public ProjectDesignPeriod ProjectDesignPeriod { get; set; }

        [ForeignKey("ProjectDesignVisitId")] public ProjectDesignVisit ProjectDesignVisit { get; set; }

        public ProjectSchedule ProjectSchedule { get; set; }
        public int? HH { get; set; }
        public int? MM { get; set; }
        public int? NoOfDay { get; set; }
        public string Message { get; set; }
        public ProjectScheduleOperator? Operator { get; set; }
    }
}