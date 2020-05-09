using GSC.Data.Entities.Common;
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
        public int[] SubjectIds { get; set; }
        public int[] VisitIds { get; set; }
        public int[] TemplateIds { get; set; }
        public int[] PeriodIds { get; set; }
        public int PdfType { get; set; }
        public int PdfStatus { get; set; }
        public bool AnnotationType { get; set; }
        public bool? IsClientLogo { get; set; }
        public bool? IsCompanyLogo { get; set; }
        public bool? IsInitial { get; set; }
        public bool? IsSponsorNumber { get; set; }
        public bool? IsScreenNumber { get; set; }
        public bool? IsSubjectNumber { get; set; }
        public decimal? LeftMargin { get; set; }
        public decimal? RightMargin { get; set; }
        public decimal TopMargin { get; set; }
        public decimal BottomMargin { get; set; }
        public int? CompanyId { get; set; }
    }

    public class CompanyData
    {
        [Key]
        public int Id { get; set; }
        public string IsComLogo { get; set; }
        public string IsClientLogo { get; set; }
        public string CompanyName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Address { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string Logo { get; set; }
        public string ClientLogo { get; set; }
        public string IsSignature { get; set; }
        public string Username{ get; set; }
    }
}