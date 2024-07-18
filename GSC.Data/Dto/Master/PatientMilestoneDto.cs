using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class PatientMilestoneDto : BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }
        public PaymentTypePatient PaymentTypePatient { get; set; }
        public decimal? visitTotal { get; set; }
        public bool PayAmountType { get; set; }
        public decimal? VisitsTotalCost { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? PaybalAmount { get; set; }
        public DateTime? DueDate { get; set; }
        [StringLength(300, ErrorMessage = "Remark Maximum 300 characters exceeded")]
        public string Remark { get; set; }
        public int? ProjectDesignVisitId { get; set; }
    }
    public class PatientMilestoneGridDto : BaseAuditDto
    {
        public string ProjectName { get; set; }
        public string PaymentTypePatient { get; set; }
        public decimal? visitTotal { get; set; }
        public decimal? VisitsTotalCost { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? PaybalAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public string Remark { get; set; }
        public string PatientCostVisits { get; set; }
        public string VisitName { get; set; }
        public bool IsInvoiceGenerated { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
