using GSC.Common.Base;
using GSC.Common.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.CTMS
{
    public class SiteTypes : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Master.Project Project { get; set; }

        public int WorkingDayId { get; set; }
        [ForeignKey("WorkingDayId")]
        public WorkingDay WorkingDay { get; set; }

        
    }
}
