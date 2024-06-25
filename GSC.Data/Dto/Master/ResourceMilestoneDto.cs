using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class ResourceMilestoneDto : BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int? CountryId { get; set; }
        public int StudyPlanTaskId { get; set; }
        public string DependentTask { get; set; }
        public DateTypeResource DateTypeResource { get; set; }
        public DateTime? DueDate { get; set; }
        public bool? PayAmountType { get; set; }
        public decimal? TasksTotalCost { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? PaybalAmount { get; set; }
        [StringLength(50, ErrorMessage = "Remark Maximum 50 characters exceeded")]
        public string Remark { get; set; }
        public decimal? ResourceTotal { get; set; }

    }
    public class ResourceMilestoneGridDto : BaseAuditDto
    {
        public string ProjectName { get; set; }
        public int? SiteId { get; set; }
        public string SitedName { get; set; }
        public string CountryName { get; set; }
        public string DateTypeResource { get; set; }
        public decimal? ResourceTotal { get; set; }
        public decimal? TasksTotalCost { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? PaybalAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public string Remark { get; set; }
        public string StudyPlanTask { get; set; }
        public bool IsInvoiceGenerated { get; set; }

    }
}
