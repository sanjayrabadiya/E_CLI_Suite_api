using System;
using System.Collections.Generic;
using GSC.Data.Dto.Project.Workflow;
using GSC.Helper;

namespace GSC.Data.Dto.ProjectRight
{
    public class ProjectDashBoardDto
    {
        public int ProjectCount { get; set; }
        public int ProjectReviewed { get; set; }
        public int ProjectPendingReview { get; set; }
        public List<ProjectDocumentReviewDto> ProjectList { get; set; }
    }

    public class ProjectDocumentReviewDto
    {
        public Entities.Master.Project Project { get; set; }
        public int Id { get; set; }
        public int ProjectDocumentId { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public bool IsReview { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string ReviewNote { get; set; }
        public string ProjectName { get; set; }
        public string ParentProjectName { get; set; }
        public int? AssignedById { get; set; }
        public string AssignedBy { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string ProjectNumber { get; set; }
        public string DocumentPath { get; set; }
        public string FileName { get; set; }

        public string MimeType { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public string IsTraning { get; set; }
        public List<ReviewDeteail> TotalReview { get; set; }
        public string TotalReviewName { get; set; }
        public List<ReviewDeteail> PendingReview { get; set; }
        public string PendingReviewName { get; set; }
        public TrainigType? TrainingType { get; set; }
        public int? TrainerId { get; set; }
        public string TrainingDuration { get; set; }
        public int[] Ids { get; set; }
        public int? AuditReasonID { get; set; }
        public string AuditReason { get; set; }
        public string RollbackReason { get; set; }
        public DateTime? RollbackOn { get; set; }
        public string RollabackBy { get; set; }
        public string AccessType { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public int? ProjectCreatedBy { get; set; }
    }

    public class ProjectDocumentHistory
    {
        public List<ReviewDeteail> TotalReview { get; set; }
        public List<ReviewDeteail> PendingReview { get; set; }
        public List<ProjectDocumentReviewDto> RollbackRights { get; set; }
    }

    public class ReviewDeteail
    {
        private DateTime? _reviewDate;
        public string UserName { get; set; }
        public string DocumentPath { get; set; }

        public DateTime? ReviewDate
        {
            get => _reviewDate?.UtcDateTime();
            set => _reviewDate = value?.UtcDateTime();
        }

        public string ReviewNote { get; set; }
        public string AssignedBy { get; set; }
        public string IsDeleted { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string TrainerName { get; set; }
        public string TrainingType { get; set; }
        public int? TrainerId { get; set; }
        public string TrainingDuration { get; set; }
    }

    public class DashboardQueryStatusDto
    {
        public string DisplayName { get; set; }
        public int? Open { get; set; }
        public int? Answered { get; set; }
        public int? Resolved { get; set; }
        public int? ReOpened { get; set; }
        public int? Closed { get; set; }
        public int? SelfCorrection { get; set; }
        public int? Total { get; set; }
        public int? Acknowledge { get; set; }
        public int? MyQuery { get; set; }
        public string QueryStatus { get; set; }
        public QueryStatus? Status { get; set; }
        public int? ScreeningEntryId { get; set; }
    }

    public class DashboardStudyStatusDto
    {
        public string DisplayName { get; set; }
        public int? NotStarted { get; set; }
        public int? InProcess { get; set; }
        public int? Submitted { get; set; }
        public int? Reviewed { get; set; }
        public int? Completed { get; set; }
        public int? Review1 { get; set; }
        public int? Review2 { get; set; }
        public int? Review3 { get; set; }
        public int? Review4 { get; set; }
        public int? Review5 { get; set; }
        public WorkFlowLevelDto WorkflowDetail { get; set; }
    }
}