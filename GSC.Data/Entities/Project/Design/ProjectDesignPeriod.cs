using System.Collections.Generic;
using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignPeriod : BaseEntity, ICommonAduit
    {
        public int ProjectDesignId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public IList<ProjectDesignVisit> VisitList { get; set; }
        public ProjectDesign ProjectDesign { get; set; }
        public int? AttendanceTemplateId { get; set; }
        public int? DiscontinuedTemplateId { get; set; }
    }
}