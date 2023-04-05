using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.Master
{
    public class ManageSiteDto : BaseDto
    {
        public string SiteName { get; set; }
        public string ContactName { get; set; }
        public string SiteEmail { get; set; }
        public string ContactNumber { get; set; }
        public string SiteAddress { get; set; }
        public bool Status { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public int? CompanyId { get; set; }
        public List<ManageSiteRole> ManageSiteRole { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public List<string> Facilities { get; set; }
        public List<ManageSiteAddress> ManageSiteAddress { get; set; }
    }

    public class ManageSiteGridDto : BaseAuditDto
    {
        public string SiteName { get; set; }
        public string ContactName { get; set; }
        public string SiteEmail { get; set; }
        public string ContactNumber { get; set; }
        public string SiteAddress { get; set; }
        public List<string> SiteAddresses { get; set; }
        public bool Status { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string TherapeuticIndicationName { get; set; }
        public string Facilities { get; set; }
    }

    public class ExperienceFillter
    {
        public int? TrialTypeId { get; set; }
        public int? DesignTrialId { get; set; }
        public int? RegulatoryId { get; set; }
        public int? InvestigatorId { get; set; }
        public int? DrugId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }


    public class ExperienceModel
    {
        public int? ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int? InvestigatorId { get; set; }
        public int? CountryId { get; set; }
        public string TypeOfTrial { get; set; }
        public string TherapeuticIndication { get; set; }
        public string DrugName { get; set; }
        public string StudyDuration { get; set; }
        public string StudyName { get; set; }
        public string StudyCode { get; set; }
        public string SiteName { get; set; }
        public List<string> SiteNames { get; set; }
        public int? NoOfSite { get; set; }
        public int? NoOfCountry { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? NumberOfPatients { get; set; }
        public int? TargetedSubject { get; set; }
        public List<int?> TargetedSubjects { get; set; }
        public string ProjectStatus { get; set; }
        public string Submission { get; set; }
        public string InvestigatorName { get; set; }
        public List<string> InvestigatorNames { get; set; }
    }
}
