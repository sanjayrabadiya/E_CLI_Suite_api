using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.ProjectRight
{
    public class ProjectRight : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsTrainingRequired { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public bool IsReviewDone { get; set; }
        public Master.Project project { get; set; }
        public string RollbackReason { get; set; }
        public int? AuditReasonId { get; set; }
        public SecurityRole role { get; set; }
    }
}