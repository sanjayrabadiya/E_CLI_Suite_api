using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.Common;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class SitePaymentDto : BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Site is required.")]
        public int SiteId { get; set; }
        public int? CountryId { get; set; }

        [Required(ErrorMessage = "BudgetPaymentType is required.")]
        public BudgetPaymentType BudgetPaymentType { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public decimal? VisitTotal { get; set; }
        public int? NoOfPatient { get; set; }
        public int? PassThroughCostActivityId { get; set; }
        public decimal? PassThroughTotalRate { get; set; }
        public BudgetFlgType? BudgetFlgType { get; set; }
        public int? UnitId { get; set; }
        public int? NoOfUnitPatient { get; set; }
        public int? Frequency { get; set; }
        public decimal? PayableAmount { get; set; }
        [StringLength(300, ErrorMessage = "Remark Maximum 300 characters exceeded")]
        public string Remark { get; set; }

    }
    public class SitePaymentGridDto : BaseAuditDto
    {
        public int  ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public string BudgetPaymentType { get; set; }
        public string Visit { get; set; }
        public decimal? VisitTotal { get; set; }
        public int? NoOfPatient { get; set; }
        public string Activity { get; set; }
        public decimal? PassThroughTotalRate { get; set; }
        public string UnitName { get; set; }
        public int? NoOfUnitPatient { get; set; }
        public int? Frequency { get; set; }
        public decimal? PayableAmount { get; set; }
        public string Remark { get; set; }
        //public List<SitePaymentGridDto?> SitePaymentChildGridDto { get; set; }

    }
}
