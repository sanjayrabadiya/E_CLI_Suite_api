using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using System;
using System.Collections.Generic;
namespace GSC.Data.Entities.CTMS
{
    public class ResourceMilestone : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int? CountryId { get; set; }
        public int StudyPlanTaskId { get; set; }
        public string DependentTask { get; set; }
        public DateTypeResource? DateTypeResource { get; set; }
        public DateTime? DueDate { get; set; }
        public bool? PayAmountType { get; set; }
        public decimal? TasksTotalCost { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? PaybalAmount { get; set; }
        public decimal? ResourceTotal { get; set; }
        public string Remark { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public Master.Project Project { get; set; }
        public Country Country { get; set; }
        public StudyPlanTask StudyPlanTask { get; set; }
        public List<ResourceMilestoneInvoice> ResourceMilestoneInvoice { get; set; }
    }
}
