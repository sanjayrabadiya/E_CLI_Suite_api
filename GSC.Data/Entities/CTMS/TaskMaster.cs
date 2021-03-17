using GSC.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class TaskMaster : BaseEntity
    {
        public string TaskName { get; set; }
        public int? ParentId { get; set; }
        public int TaskTemplateId { get; set; }
        public int TaskOrder { get; set;}
        public bool IsMileStone { get; set; }
        public int Duration { get; set; }

    }
}
