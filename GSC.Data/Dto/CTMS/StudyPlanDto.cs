using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
    public class StudyPlanDto: BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Template is required.")]
        public int TaskTemplateId { get; set; }
        [Required(ErrorMessage = "Start Date is required.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End Date is required.")]
        public DateTime EndDate { get; set; }
        public int? CurrencyId {  get; set; }
        public bool IfPlanApproval { get; set; }
        public bool IfBudgetApproval { get; set; }
        public List<CurrencyRateDto> CurrencyRateList { get; set; }
    }
    public class CurrencyRateDto
    {
        public int? localCurrencyId { get; set; }
        public decimal? localCurrencyRate { get; set; }
    }
    public class ApprovalPlanHistory : BaseDto
    {
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public bool? IsApproval { get; set; }
        public string ReasonOth { get; set; }
        public string ReasonName { get; set; }
        public string ApprovalRole { get; set; }
        public string ApprovalBy { get; set; }
        public DateTime? ApprovalOn { get; set; }
        public string TimeZone { get; set; }
        public string IpAddress { get; set; }
    }
}
