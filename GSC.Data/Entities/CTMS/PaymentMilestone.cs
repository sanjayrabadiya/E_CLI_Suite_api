using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using System.Collections.Generic;
namespace GSC.Data.Entities.CTMS
{
    public class PaymentMilestone : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int? CountryId { get; set; } 
        public MilestoneType MilestoneType { get; set; }
        public PaymentType PaymentType { get; set; }
        public int? PatientCount { get; set; }
        public int? PatientStatus { get; set; }
        public decimal? EstimatedRevenue { get; set; }
        public decimal? PaidRevenue { get; set; }
        public decimal? TotalRevenue { get; set; }
        public decimal?  TotalPerPatient { get; set; }
        public bool IsApproved { get; set; }
        public bool IsSendBack { get; set; }
        public Master.Project Project { get; set; }
        public Country Country { get; set; }
        public List<PaymentMilestoneTaskDetail> PaymentMilestoneTaskDetails { get; set; } = null;
        public List<PaymentMilestoneVisitDetail> PaymentMilestoneVisitDetails { get; set; } = null;
    }
}
