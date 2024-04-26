using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using System;
using System.Collections.Generic;
namespace GSC.Data.Entities.CTMS
{
    public class PassthroughMilestone : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public PaymentTypePassThrough PaymentTypePassThrough { get; set; }
        public bool? PayAmountType { get; set; }
        public decimal? PassThroughTotal { get; set; }
        public decimal? PassThroughActivityTotal { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? PaybalAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public string Remark { get; set; }
        public Master.Project Project { get; set; }
        public List<PaymentMilestonePassThroughDetail> PaymentMilestonePassThroughDetail { get; set; } = null;
    }
}
