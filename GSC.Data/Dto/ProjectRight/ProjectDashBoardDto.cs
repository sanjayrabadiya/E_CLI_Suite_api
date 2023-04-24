using System;
using System.Collections.Generic;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Entities.Attendance;
using GSC.Helper;
using GSC.Shared.Extension;

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
        private DateTime? _reviewDate;
        public DateTime? ReviewDate
        {
            get => _reviewDate?.UtcDateTime();
            set => _reviewDate = value?.UtcDateTime();
        }
        public string ReviewNote { get; set; }
        public string ProjectName { get; set; }
        public string ParentProjectCode { get; set; }
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
        public bool IsRevoke { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public int? ProjectCreatedBy { get; set; }

        public string TrainingTypeName { get; set; }
        public string TrainerName { get; set; }
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
        public double? Avg { get; set; }
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

    public class DashboardPatientStatusDisplayDto
    {
        public string ProjectName { get; set; }
        public string DisplayName { get; set; }
        public int Id { get; set; }
        public double Avg { get; set; }
    }

    public class DashboardPatientStatusDto
    {
        public string ProjectName { get; set; }
        public int? Target { get; set; }
        public bool IsParentProject { get; set; }
        public int? ParentProjectTarget { get; set; }
        public int ProjectId { get; set; }
        public List<DashboardPatientStatusDisplayDto> StatusList { get; set; }
    }

    public class DashboardRecruitmentStatusDisplayDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string DisplayName { get; set; }
        public string ScreeningMonth { get; set; }
        public int ScreeningMonthNo { get; set; }
        public string RandomizationMonth { get; set; }
        public int RandomizationMonthNo { get; set; }
        public int ScreeningDataCount { get; set; }
        public int RandomizationDataCount { get; set; }
        public DateTime? ScreeningDate { get; set; }
        public DateTime? RandomizationDate { get; set; }
        public double Avg { get; set; }
    }

    public class DashboardRecruitmentRateDto
    {
        public int? ScreeningDataCount { get; set; }
        public int? ScreeningAvgValue { get; set; }
        public int? ScreeningMonth { get; set; }
        public int? RandomizationDataCount { get; set; }
        public int? RandomizationAvgValue { get; set; }
        public int? RandomizationMonth { get; set; }
        public bool IsScreeningAchive { get; set; }
        public bool IsRandomizationAchive { get; set; }
    }

    public class DashboardProject
    {
        public int ProjectId { get; set; }
        public ProjectGridDto Project { get; set; }
        public List<string> CountriesName { get; set; }
        public int? CountCountry { get; set; }
        public string projectCode { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DashboardInformConsentStatusDto
    {
        public int ProjectId { get; set; }
        public int PreScreening { get; set; }
        public int Screened { get; set; }
        public int ConsentInProgress { get; set; }
        public int ConsentCoSign { get; set; }
        public int ConsentCompleted { get; set; }
        public int ReConsent { get; set; }
        public int ConsentWithdraw { get; set; }
        public int TotalRandomization { get; set; }
        public int? Total { get; set; }
        public string DisplayName { get; set; }
    }
    public class DashboardQueryGraphDto
    {
        public int Id { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public string Lable { get; set; }

        public int week { get; set; }
    }
    public class DashboardQueryGraphFinalDto
    {
        public int Count { get; set; }
        public string Lable { get; set; }

       
    }
    public class CtmsMonitoringStatusChartDto
    {
        public MonitoringSiteStatus? Status { get; set; }
        public string StatusName { get; set; }
        public string ActivityName { get; set; }
        public int ACount { get; set; }
        public int RCount { get; set; }

        public int TerminatedCount { get; set; }

        public int OnHoldCount { get; set; }

        public int CloseOutCount { get; set; }

        public int EntrollCount { get; set; }
    }

    public class CtmsMonitoringStatusPIChartDto
    {
        public string Lable { get; set; }
        public int Count { get; set; }
        public string Text { get; set; }

        public string Status { get; set; }
    }
    public class CtmsMonitoringPlanDashoardDto
    {
        public int Id { get; set; }
        public string Activity { get; set; }
        public DateTime? ScheduleStartDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public string Status { get; set; }
        public string visitStatus { get; set; }
        public string Country { get; set; }
        public string Site { get; set; }
    }

    public class DynamicAeChart
    {
        public List<DynamicAeChartDetails> Data { get; set; }
        public string SeriesName { get; set; }

    }

    public class DynamicAeChartDetails
    {
        public string X { get; set; }
        public int Y { get; set; }
    }

    public class DynamicAeChartData
    {
        public string VariableName { get; set; }
        public string VariableValue { get; set; }
        public string Against { get; set; }
        public int ScreeningTemplateId { get; set; }
        public string Value { get; set; }
        public int ProjectDesignVariableId { get; set; }
    }

}