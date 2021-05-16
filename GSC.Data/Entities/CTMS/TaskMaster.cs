using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class TaskMaster : BaseEntity, ICommonAduit
    {
        public string TaskName { get; set; }
        public int? ParentId { get; set; }
        public int TaskTemplateId { get; set; }
        public int TaskOrder { get; set;}
        public bool IsMileStone { get; set; }
        public int Duration { get; set; }
        public int? DependentTaskId { get; set; }
        public ActivityType? ActivityType { get; set; }
        public int OffSet { get; set; }
        public RefrenceType? RefrenceType { get; set; }

    }
}
