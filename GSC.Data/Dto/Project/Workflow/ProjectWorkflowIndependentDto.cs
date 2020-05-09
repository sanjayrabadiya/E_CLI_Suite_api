using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Workflow
{
    public class ProjectWorkflowIndependentDto : BaseDto
    {
        public int ProjectWorkflowId { get; set; }

        [Required(ErrorMessage = "Security Role is required.")]
        public int SecurityRoleId { get; set; }

        public bool IsDataEntryUser { get; set; }
        public bool IsStartTemplate { get; set; }
        public bool IsWorkFlowBreak { get; set; }
        public bool IsGenerateQuery { get; set; }
    }
}