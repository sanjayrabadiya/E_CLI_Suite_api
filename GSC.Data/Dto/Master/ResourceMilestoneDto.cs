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
        public PaymentTypeResource PaymentTypeResource { get; set; }
        public bool PayAmountType { get; set; }
        public decimal? TasksTotalCost { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? PaybalAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? ResourceTotal { get; set; }
        public int[] StudyPlanTaskIds { get; set; }

    }
    public class ResourceMilestoneGridDto : BaseAuditDto
    {
        public string ProjectName { get; set; }
        public int? SiteId { get; set; }
        public string SitedName { get; set; }
        public string CountryName { get; set; }
        public string PaymentTypeResource { get; set; }
        public decimal? ResourceTotal { get; set; }
        public decimal? TasksTotalCost { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? PaybalAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public string StudyPlanTasks { get; set; }

    }
}
