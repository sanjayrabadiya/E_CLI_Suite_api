using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class PassthroughMilestoneDto : BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }
        public PaymentTypePassThrough PaymentTypePassThrough { get; set; }
        public decimal? PassThroughTotal { get; set; }
        public decimal? PassThroughActivityTotal { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? PaybalAmount { get; set; }
        public DateTime? DueDate { get; set; }
        [StringLength(50, ErrorMessage = "Remark Maximum 50 characters exceeded")]
        public string Remark { get; set; }
        public int? PassThroughCostActivityId { get; set; }
    }
    public class PassthroughMilestoneGridDto : BaseAuditDto
    {
        public string ProjectName { get; set; }
        public string PaymentTypePassThrough { get; set; }
        public decimal? PassThroughTotal { get; set; }
        public decimal? PassThroughActivityTotal { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? PaybalAmount { get; set; }
        public string PassThroughCostActivity { get; set; }
        public DateTime? DueDate { get; set; }
        public string Remark { get; set; }

    }
}
