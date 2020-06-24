﻿using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
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

        public RegulatoryType? RegulatoryType { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        //   public int? CompanyId { get; set; }

        public string ParentProjectName { get; set; }
        public string DesignTrialName { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string AreaName { get; set; }
        public string ClientName { get; set; }
        public string DrugName { get; set; }
        public string RegulatoryTypeName { get; set; }
        public int TrialTypeId { get; set; }
        public string TrialTypeName { get; set; }
        public int? ProjectDesignId { get; set; }
        public bool IsStatic { get; set; }
        public int? AttendanceLimit { get; set; }

        public int? InvestigatorContactId { get; set; }
        public int? NoofSite { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool Locked { get; set; }
    }

    public class ProjectDetailsDto 
    {
        public SiteDetailsDto siteDetails { get; set; }
        public DesignDetailsDto designDetails { get; set; }
        public WorkflowDetailsDto workflowDetails { get; set; }
        public UserRightDetailsDto userRightDetails { get; set; }
        public SchedulesDetailsDto schedulesDetails { get; set; }
        public EditCheckDetailsDto editCheckDetails { get; set; }
    }

    public class SiteDetailsDto
    {
        public int? NoofSite { get; set; }
        public int? NoofCountry { get; set; }
        public bool MarkAsCompleted { get; set; }
    }

    public class DesignDetailsDto
    {
        public int? NoofPeriod { get; set; }
        public int? NoofVisit { get; set; }
        public int? NoofECrf { get; set; }
        public bool? MarkAsCompleted { get; set; }
    }

    public class WorkflowDetailsDto
    {
        public int? Independent { get; set; }
        public int? NoofLevels { get; set; }       
        public bool? MarkAsCompleted { get; set; }
    }
    public class UserRightDetailsDto
    {       
        public int? NoofUser { get; set; }
        public bool? MarkAsCompleted { get; set; }
    }

    public class SchedulesDetailsDto
    {
        public int? NoofVisit { get; set; }
        public bool? MarkAsCompleted { get; set; }
    }

    public class EditCheckDetailsDto
    {
        public int? NoofRules { get; set; }
        public int? NoofFormulas { get; set; }
        public bool? MarkAsCompleted { get; set; }
    }
}