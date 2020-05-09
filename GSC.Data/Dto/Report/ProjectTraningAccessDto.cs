using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.Report
{
    public class ProjectTrainigAccessSearchDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int?[] RoleIds { get; set; }
        public int?[] UserIds { get; set; }
    }
   
    public class ProjectAccessDto : BaseDto
    {
        public int? ProjectId { get; set; }
        public int? ParentProjectId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string SiteName { get; set; }
        public string UserName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string RoleName { get; set; }
        public int? AssignedById { get; set; }
        public string AssignedBy { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string AuditReason { get; set; }
        public string RollbackReason { get; set; }
        public DateTime? RollbackOn { get; set; }
        public string RollabackBy { get; set; }
        public string AccessType { get; set; }
        public int? AuditReasonID { get; set; }
        public string CreatedByName { get; set; }
        public string DocumentName { get; set; }
        public int? TrainerId { get; set; }
        public int? CreatedBy { get; set; }
    }

    public class ProjectTrainingDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string SiteName { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public int? AssignedById { get; set; }
        public string AssignedBy { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string DocumentName { get; set; }
        public bool IsReview { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string TrainerName { get; set; }
        public string TrainingDuration { get; set; }
        public string ReviewNote { get; set; }
        public string TrainingType { get; set; }
        public int? TrainerId { get; set; }
    }
}
