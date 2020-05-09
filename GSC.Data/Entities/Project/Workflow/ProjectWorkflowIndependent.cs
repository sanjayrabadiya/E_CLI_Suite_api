using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Project.Workflow
{
    public class ProjectWorkflowIndependent : BaseEntity
    {
        public int ProjectWorkflowId { get; set; }
        public int SecurityRoleId { get; set; }
        public bool IsDataEntryUser { get; set; }
        public bool IsStartTemplate { get; set; }
        public bool IsWorkFlowBreak { get; set; }
        public bool IsGenerateQuery { get; set; }
        public ProjectWorkflow ProjectWorkflow { get; set; }
    }
}