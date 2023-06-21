using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Project.Workflow
{
    public class WorkflowVisit : BaseEntity, ICommonAduit
    {
        public bool IsIndependent { get; set; }
        public int? ProjectWorkflowIndependentId { get; set; }
        public int? ProjectWorkflowLevelId { get; set; }
        public int ProjectDesignVisitId { get; set; }

        public ProjectWorkflowLevel ProjectWorkflowLevel { get; set; }
    }
}
