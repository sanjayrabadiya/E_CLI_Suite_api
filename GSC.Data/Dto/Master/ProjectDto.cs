using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class ProjectDto : BaseDto
    {
        public string ProjectCode { get; set; }

        [Required(ErrorMessage = "Project Name is required.")]
        public string ProjectName { get; set; }

        public string ProjectNumber { get; set; }

        public int? ParentProjectId { get; set; }

        [Required(ErrorMessage = "Design Trial is required.")]
        public int DesignTrialId { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public int CountryId { get; set; }

        [Required(ErrorMessage = "Client is required.")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Drug is required.")]
        public int DrugId { get; set; }

        public int? CityAreaId { get; set; }
        public int? CityId { get; set; }
        public int? StateId { get; set; }
        public string SiteName { get; set; }
        public string PinCode { get; set; }

        [Required(ErrorMessage = "Period is required.")]
        public int Period { get; set; }

        public int? RegulatoryTypeId { get; set; }
        public RegulatoryType? RegulatoryType { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string ParentProjectName { get; set; }
        public string RegulatoryTypeName { get; set; }
        public int TrialTypeId { get; set; }
        public int? ProjectDesignId { get; set; }
        public bool IsStatic { get; set; }
        public int? AttendanceLimit { get; set; }
        public int? InvestigatorContactId { get; set; }
        public int? NoofSite { get; set; }
        public int? CompanyId { get; set; }
        public bool Locked { get; set; }
        public int? ManageSiteId { get; set; }
        public bool IsTestSite { get; set; }
        public bool IsSendSMS { get; set; }
        public bool IsSendEmail { get; set; }
        public int? Recruitment { get; set; }
    }



    public class ProjectGridDto : BaseAuditDto
    {
        public bool Locked { get; set; }
        public int? ProjectDesignId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string ProjectNumber { get; set; }
        public int? ParentProjectId { get; set; }
        public string SiteName { get; set; }
        public string PinCode { get; set; }
        public int Period { get; set; }
        public string ParentProjectCode { get; set; }
        public string RegulatoryTypeName { get; set; }
        public bool IsStatic { get; set; }
        public bool IsTestSite { get; set; }
        public int? AttendanceLimit { get; set; }
        public int? NoofSite { get; set; }
        public string DesignTrialName { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string AreaName { get; set; }
        public string ClientName { get; set; }
        public string DrugName { get; set; }
        public string InvestigatorContactName { get; set; }
        public string TherapeuticIndication { get; set; }


        public bool? AnyLive { get; set; }
        public string TrialVersion { get; set; }
        public string LiveVersion { get; set; }

        public string ProjectStatusName { get; set; }

        public bool? IsCtmsStudy { get; set; }
    }

    public class ProjectDetailsDto
    {
        public string SendSMS { get; set; }
        public string SendEmail { get; set; }
        public string RandomizationAutomatic { get; set; }
        public DesignDetailsDto DesignDetails { get; set; }
        public UserRightDetailsDto UserRightDetails { get; set; }
        public SchedulesDetailsDto SchedulesDetails { get; set; }
        public List<BasicSiteDto> Sites { get; set; }
        public List<BasicWorkFlowDetailsDto> WorkFlowDetail { get; set; }
        public EditCheckDetailsDto EditCheckDetails { get; set; }
        public string CountryName { get; set; }
        public string TrialTypeName { get; set; }
        public string RegulatoryTypeName { get; set; }
    }

    public class SiteDetailsDto
    {
        public int? NoofSite { get; set; }
        public int? NoofCountry { get; set; }
        public bool MarkAsCompleted { get; set; }
    }


    public class BasicWorkFlowDetailsDto
    {
        public string RoleName { get; set; }
        public int LevelNo { get; set; }
    }

    public class BasicSiteDto
    {
        public string SiteName { get; set; }
        public string SiteCode { get; set; }
        public string SiteCountry { get; set; }
    }

    public class DesignDetailsDto
    {
        public double GoLiveVersion { get; set; }
        public double TrialVersion { get; set; }
        public int? NoofPeriod { get; set; }
        public int? NoofVisit { get; set; }
        public int? NoofECrf { get; set; }
        public int NoofTemplate { get; set; }
    }

    public class WorkflowDetailsDto
    {
        public int? Independent { get; set; }
        public int? NoofLevels { get; set; }
        public bool? MarkAsCompleted { get; set; }
    }
    public class UserRightDetailsDto
    {
        public int NoofUser { get; set; }
        public int NoOfDocument { get; set; }
        public int DocumentNotReview { get; set; }
    }

    public class SchedulesDetailsDto
    {
        public int NoofVisit { get; set; }
        public int NoOfReferenceTemplate { get; set; }
        public int NoOfTargetTemplate { get; set; }
    }

    public class EditCheckDetailsDto
    {
        public int NoofRules { get; set; }
        public int NoofFormulas { get; set; }
        public int NotVerified { get; set; }
        public bool IsAnyRecord { get; set; }
    }


    public class StydyDetails
    {
        [Required(ErrorMessage = "Project Code is required.")]
        public string StudyCode { get; set; }
        public int NoofStudy { get; set; }
        public int NoofSites { get; set; }
        [Required(ErrorMessage = "Valid From is required.")]
        public DateTime ValidFrom { get; set; }
        [Required(ErrorMessage = "Valid Tos is required.")]
        public DateTime ValidTo { get; set; }
        [Required(ErrorMessage = "Company ID is required.")]
        public int CompanyID { get; set; }
    }

    public class SMSEMailConfig
    {
        public int Id { get; set; }
        public bool IsSendSMS { get; set; }
        public bool IsSendEmail { get; set; }
    }

    public class LockUnlockProject
    {
        public int ProjectId { get; set; }
        public ProjectGridDto Project { get; set; }
        public List<string> CountriesName { get; set; }
        public int? CountCountry { get; set; }
        public string projectCode { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ProjectStatusDto : BaseDto
    {
        public int ProjectId { get; set; }
        public ProjectStatusEnum projectStatusId { get; set; }

        public int AuditReasonId { get; set; }

        public string ReasonOth { get; set; }
    }

    public class CloneProjectDto
    {
        public int CloneProjectId { get; set; }
        public bool EditcheckClone { get; set; }

        public bool ScheduleClone { get; set; }

        public bool WorkflowClone { get; set; }
    }

}