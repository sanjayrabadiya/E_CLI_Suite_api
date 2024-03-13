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
       
        public int? CompanyId { get; set; }
        public int? PdfType { get; set; }
        public DossierPdfStatus? PdfStatus { get; set; }
        public bool AnnotationType { get; set; }
        public bool? IsClientLogo { get; set; }
        public bool? IsCompanyLogo { get; set; }
        public bool? IsInitial { get; set; }
      
        public bool? IsScreenNumber { get; set; }
        public bool? IsSubjectNumber { get; set; }
        
        public decimal? LeftMargin { get; set; }
        public decimal? RightMargin { get; set; }
        public decimal? TopMargin { get; set; }
        public decimal? BottomMargin { get; set; }
        public int? TimezoneoffSet { get; set; }
        
        public bool? NonCRF { get; set; }
        public bool? IsSectionDisplay { get; set; }
       
        public bool? IsSync { get; set; }


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
    public class RandomizationIwrsReport
    {
        public int ProjectId { get; set; }
        public int SiteId { get; set; }
        public int[] VisitIds { get; set; }
    }
    public class RandomizationIwrsReportData
    {
        public string ProjectCode { get; set; }
        public string SiteCode { get; set; }
        public string Visit { get; set; }
        public string Treatment { get; set; }
        public string KitNo { get; set; }
        public string ScreeningNo { get; set; }
        public string RandomizationNumber { get; set; }
        public DateTime? RandomizationDate { get; set; }

        public int ProjectId { get; set; }

        public int? RandomizationId { get; set; }

        public int SiteId { get; set; }

        public int? VisitId { get; set; }

        public string AllocatedBy { get; set; }

        public DateTime? Allocatedate { get; set; }

        public int? ActionBy { get; set; }
    }


    public class ProductAccountabilityCentralReport
    {
        public int Id { get; set; }
        public string ProjectCode { get; set; }
        public string ProductTypeCode { get; set; }
        public string KitNo { get; set; }
        public string VisitName { get; set; }
        public string SiteCode { get; set; }
        public string ActionName { get; set; }
        public int NoofBoxorBottle { get; set; }
        public int Noofimp { get; set; }
        public string StorageLocation { get; set; }
        public string StorageConditionTemprature { get; set; }
        public int RetentionQty { get; set; }

        public int RequestedQty { get; set; }
        public int UsedVerificationQty { get; set; }
        public string LotBatchNo { get; set; }
        public DateTime? RetestExpiryDate { get; set; }

        public string RetestExpiryDatestr { get; set; }

        public ReTestExpiry? RetestExpiryId { get; set; }
        public string Comments { get; set; }
        public int TotalIMP { get; set; }
        public string ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }
        public string ReceiptStatus { get; set; }
        public int SiteId { get; set; }
        public int StudyProductTypeId { get; set; }
        public KitStatus Status { get; set; }

        public KitStatus? PreStatus { get; set; }

        public string KitStatus { get; set; }

        public int? ToSiteId { get; set; }

        public string RequestedFrom { get; set; }
        public string RequestedTo { get; set; }

        public string Type { get; set; }

        public string CourierName { get; set; }

        public string TrackingNumber { get; set; }

        public int ImpPerKit { get; set; }

        public string RoleName { get; set; }

        public string ActionDatestr { get; set; }

        public string RandomizationNo { get; set; }

        public string ScreeningNo { get; set; }

        public bool IsRetension { get; set; }
    }

    public class ProductAccountabilityCentralReportSearch
    {
        public int ProjectId { get; set; }
        public int SiteId { get; set; }
        public int productTypeId { get; set; }
        public ProductAccountabilityActions ActionType { get; set; }
        public string LotNo { get; set; }

        public int VisitId { get; set; }


    }

    public class KitHistoryReportSearchModel
    {
        public int ProjectId { get; set; }
        public int KitId { get; set; }
        public int RandomizationId { get; set; }
        public KitHistoryReportType Type { get; set; }

    }
}