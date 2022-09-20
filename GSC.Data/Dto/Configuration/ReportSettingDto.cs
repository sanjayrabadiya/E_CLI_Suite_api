using GSC.Data.Dto.Master;
using GSC.Data.Dto.Report.Pdf;
using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Configuration
{
    public class ReportSettingDto : BaseDto
    {
        public bool IsClientLogo { get; set; }
        public bool IsCompanyLogo { get; set; }
        public bool IsInitial { get; set; }
        public bool IsSponsorNumber { get; set; }
        public bool IsScreenNumber { get; set; }
        public bool IsSubjectNumber { get; set; }
        public decimal LeftMargin { get; set; }
        public decimal RightMargin { get; set; }
        public decimal TopMargin { get; set; }
        public decimal BottomMargin { get; set; }
    }
    public class ReportSettingNew : BaseDto
    {
        public int ProjectDesignReportSettingId { get; set; }
        public int ProjectId { get; set; }
        public int? CountryId { get; set; }
        public int[] SiteId { get; set; }
        public List<DropDownDto> SubjectIds { get; set; }
        public int[] VisitIds { get; set; }
        public int[] TemplateIds { get; set; }
        public int[] PeriodIds { get; set; }
        public int PdfType { get; set; }
        public DossierPdfStatus PdfStatus { get; set; }
        public bool AnnotationType { get; set; }
        public bool? IsClientLogo { get; set; }
        public bool? IsCompanyLogo { get; set; }
        public bool? IsInitial { get; set; }
        public bool? IsSponsorNumber { get; set; }
        public bool? IsScreenNumber { get; set; }
        public bool? IsSubjectNumber { get; set; }
        public bool? IsSiteCode { get; set; }
        public decimal? LeftMargin { get; set; }
        public decimal? RightMargin { get; set; }
        public decimal? TopMargin { get; set; }
        public decimal? BottomMargin { get; set; }
        public int? CompanyId { get; set; }
        public int? TimezoneoffSet { get; set; }
        public string ClientDateTime { get; set; }
        public bool? NonCRF { get; set; }
        public bool IsSectionDisplay { get; set; }
        public string ReportCode { get; set; }
        public int SitesId { get; set; }
        public bool IsSync { get; set; }

        public CRFTypes CRFType { get; set; }

        public PdfLayouts PdfLayouts { get; set; }

        public bool WithDocument { get; set; }
    }
    public class ReportVisitsDto
    {
        public int[] VisitIds { get; set; }
    }

    public class ScreeningReportSetting : BaseDto
    {
        public int ProjectId { get; set; }
        public int? StudyId { get; set; }
        public int? VolunteerId { get; set; }
        public string ScreeningDate { get; set; }
        //public int? ProjectDesignId { get; set; }
        public int? CompanyId { get; set; }
        public int? PdfType { get; set; }
        public DossierPdfStatus? PdfStatus { get; set; }
        public bool AnnotationType { get; set; }
        public bool? IsClientLogo { get; set; }
        public bool? IsCompanyLogo { get; set; }
        public bool? IsInitial { get; set; }
        //public bool? IsSponsorNumber { get; set; }
        public bool? IsScreenNumber { get; set; }
        public bool? IsSubjectNumber { get; set; }
        //public bool? IsSiteCode { get; set; }
        public decimal? LeftMargin { get; set; }
        public decimal? RightMargin { get; set; }
        public decimal? TopMargin { get; set; }
        public decimal? BottomMargin { get; set; }
        public int? TimezoneoffSet { get; set; }
        //public string ClientDateTime { get; set; }
        public bool? NonCRF { get; set; }
        public bool? IsSectionDisplay { get; set; }
        //public string ReportCode { get; set; }
        //public int SitesId { get; set; }
        public bool? IsSync { get; set; }

        //public CRFTypes CRFType { get; set; }

        public PdfLayouts? PdfLayouts { get; set; }
    }

    public class ScreeningPdfReportDto
    {
        public string ScreeningNumber { get; set; }
        public string Initial { get; set; }
        public string VolunteerNumber { get; set; }
        public int VolunteerId { get; set; }
        public DateTime ScreeningDate { get; set; }
        public ProjectDetails ProjectDetails { get; set; }
        public List<ProjectDesignPeriodReportDto> Period { get; set; }
    }
}