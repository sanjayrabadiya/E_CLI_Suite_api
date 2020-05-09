using System.Collections.Generic;

namespace GSC.Data.Dto.ProjectRight
{
    public class ProjectRightDto
    {
        public string Name { get; set; }

        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsReviewDone { get; set; }
        public bool IsTrainingRequired { get; set; }
        public bool IsSelected { get; set; }
        public string RollbackReason { get; set; }
        public int? AuditReasonId { get; set; }
        public List<ProjectRightDto> users { get; set; }
    }

    public class ProjectRightSaveDto
    {
        public int[] Ids { get; set; }
        public int projectId { get; set; }
        public List<ProjectRightDto> projectRightDto { get; set; }
    }
}