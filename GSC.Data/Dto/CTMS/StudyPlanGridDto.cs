using GSC.Data.Entities.Common;
using System;

namespace GSC.Data.Dto.CTMS
{
    public class StudyPlanGridDto : BaseAuditDto
    {
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string GlobalCurrency { get; set; }
        public string GlobalCurrencySymbol { get; set; }
        public decimal? TotalCost { get; set; }
    }
}
