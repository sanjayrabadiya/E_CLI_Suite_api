using GSC.Data.Entities.Common;
using GSC.Helper;
using System.ComponentModel.DataAnnotations;
namespace GSC.Data.Dto.CTMS
{
    public class BudgetPaymentFinalCostDto : BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "MilestoneType is required.")]
        public MilestoneType MilestoneType { get; set; }

        [Required(ErrorMessage = "TotalAmount is required.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Percentage is required.")]
        public decimal Percentage { get; set; }
        [Required(ErrorMessage = "FinalTotalAmount is required.")]
        public decimal FinalTotalAmount { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public decimal? ProfessionalCostAmount { get; set; }
        public decimal? PatientCostAmount { get; set; }
        public decimal? PassThroughCost { get; set; }
    }

    public class BudgetPaymentFinalCostGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }
        public string MilestoneType { get; set; }
        public string MilestoneTypeName { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? FinalTotalAmount { get; set;}
        public string GlobleCurrencySymbol { get; set; }
    }
}
