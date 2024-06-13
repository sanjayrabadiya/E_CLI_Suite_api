using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using System;
using System.Collections.Generic;
namespace GSC.Data.Entities.CTMS
{
    public class PatientMilestone : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public PaymentTypePatient PaymentTypePatient { get; set; }
        public bool? PayAmountType { get; set; }
        public decimal? visitTotal { get; set; }
        public decimal? visitsTotalCost { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? PaybalAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public string Remark { get; set; }
        public Master.Project Project { get; set; }
        public int? PatientCostId { get; set; }
        public PatientCost PatientCost { get; set; }
    }
}
