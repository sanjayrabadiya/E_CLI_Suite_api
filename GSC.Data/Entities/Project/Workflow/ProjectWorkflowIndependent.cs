using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.Project.Workflow
{
    public class ProjectWorkflowIndependent : BaseEntity, ICommonAduit
    {
        public int ProjectWorkflowId { get; set; }
        public int SecurityRoleId { get; set; }
        public bool IsDataEntryUser { get; set; }
        public bool IsStartTemplate { get; set; }
        public bool IsWorkFlowBreak { get; set; }
        public bool IsGenerateQuery { get; set; }
        public ProjectWorkflow ProjectWorkflow { get; set; }
        public SecurityRole SecurityRole { get; set; }
    }
}