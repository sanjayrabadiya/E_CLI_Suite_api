using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Data.Entities.CTMS
{
    public class TaskMaster : BaseEntity, ICommonAduit
    {
        public string TaskName { get; set; }
        public int? ParentId { get; set; }
        public int TaskTemplateId { get; set; }
        public int TaskOrder { get; set; }
        public bool IsMileStone { get; set; }
        public int Duration { get; set; }
        public int? DependentTaskId { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public ActivityType? ActivityType { get; set; }
        public int OffSet { get; set; }
        public List<RefrenceTypes> RefrenceTypes { get; set; } = null;
    }
}
