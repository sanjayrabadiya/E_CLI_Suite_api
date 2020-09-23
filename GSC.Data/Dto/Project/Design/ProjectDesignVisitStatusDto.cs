using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignVisitStatusDto : BaseDto
    {
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int VisitStatusId { get; set; }
    }
}
