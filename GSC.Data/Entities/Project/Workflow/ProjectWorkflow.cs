using System.Collections.Generic;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Entities.Project.Workflow
{
    public class ProjectWorkflow : BaseEntity
    {
        public int ProjectDesignId { get; set; }
        public bool IsIndependent { get; set; }
        public int? CompanyId { get; set; }
        public ProjectDesign ProjectDesign { get; set; }
        public IList<ProjectWorkflowLevel> Levels { get; set; }
        public IList<ProjectWorkflowIndependent> Independents { get; set; }
    }
}