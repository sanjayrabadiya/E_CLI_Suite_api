using System.Collections.Generic;
using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVisit : BaseEntity, ICommonAduit
    {
        public int ProjectDesignPeriodId { get; set; }
        public ProjectDesignPeriod ProjectDesignPeriod { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsRepeated { get; set; }
        public bool? IsSchedule { get; set; }
        public IList<ProjectDesignTemplate> Templates { get; set; }
       
    }
}