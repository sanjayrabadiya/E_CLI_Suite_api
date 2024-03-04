using GSC.Data.Entities.Common;
using GSC.Helper;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class PaymentMilestoneDto : BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int? CountryId { get; set; }
        [Required]
        public MilestoneType MilestoneType { get; set; }
        public PaymentType PaymentType { get; set; }
        public decimal? EstimatedRevenue { get; set; }
        public decimal? PaidRevenue { get; set; }
        public decimal? TotalRevenue { get; set; }
        public decimal? TotalPerPatient { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsSendBack { get; set; }
        public int? PatientCount { get; set; }
        public int? PatientStatus { get; set; }
        public int[] StudyPlanTaskIds { get; set; }
        public int[] PatientCostIds { get; set; }
    }
    public class PaymentMilestoneGridDto : BaseAuditDto
    {
        public string ProjectName { get; set; }
        public int? SiteId { get; set; }
        public string SitedName { get; set; }
        public string CountryName { get; set; }
        public string MilestoneType { get; set; }
        public string PaymentType { get; set; }
        public decimal? EstimatedRevenue { get; set; }
        public decimal? PaidRevenue { get; set; }
        public decimal? TotalRevenue { get; set; }
        public decimal? TotalPerPatient { get; set; }
        public string StudyPlanTasks { get; set; }
        public int? PatientCount { get; set; }
        public string PatientStatus { get; set; }
        public string PatientCostVisits { get; set; }

    }
}
