using GSC.Common.Base;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.Project.Workflow
{
    public class ProjectWorkflowLevel : BaseEntity
    {
        public int ProjectWorkflowId { get; set; }
        public short LevelNo { get; set; }
        public int SecurityRoleId { get; set; }
        public bool IsElectricSignature { get; set; }
        public bool IsDataEntryUser { get; set; }
        public bool IsStartTemplate { get; set; }
        public bool IsWorkFlowBreak { get; set; }
        public bool IsGenerateQuery { get; set; }
        public bool IsLock { get; set; }
        public SecurityRole SecurityRole { get; set; }
        public ProjectWorkflow ProjectWorkflow { get; set; }
    }
}