using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Entities.Project.Schedule
{
    public class ProjectSchedule : BaseEntity
    {
        public int ProjectId { get; set; }
        public int ProjectDesignId { get; set; }
        public int ProjectDesignPeriodId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int? CompanyId { get; set; }
        public string AutoNumber { get; set; }
        [ForeignKey("ProjectId")] public Master.Project Project { get; set; }

        [ForeignKey("ProjectDesignId")] public ProjectDesign ProjectDesign { get; set; }

        [ForeignKey("ProjectDesignPeriodId")] public ProjectDesignPeriod ProjectDesignPeriod { get; set; }

        [ForeignKey("ProjectDesignVisitId")] public ProjectDesignVisit ProjectDesignVisit { get; set; }

        [ForeignKey("ProjectDesignTemplateId")]
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }

        [ForeignKey("ProjectDesignVariableId")]
        public ProjectDesignVariable ProjectDesignVariable { get; set; }

        public IList<ProjectScheduleTemplate> Templates { get; set; }
    }
}