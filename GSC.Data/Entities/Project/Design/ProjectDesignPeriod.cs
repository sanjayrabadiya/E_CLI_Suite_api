﻿using System.Collections.Generic;
using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignPeriod : BaseEntity
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