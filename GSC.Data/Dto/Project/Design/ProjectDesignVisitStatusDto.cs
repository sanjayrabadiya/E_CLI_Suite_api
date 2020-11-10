using GSC.Data.Entities.Common;
using GSC.Helper;
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
        public ScreeningVisitStatus VisitStatusId { get; set; }
    }

    public class ProjectDesignVisitStatusGridDto : BaseAuditDto
    {
        public string VisitName { get; set; }
        public string ProjectDesignTemplateName { get; set; }
        public string ProjectDesignVariableName { get; set; }
        public string VisitStatus { get; set; }
    }

}
