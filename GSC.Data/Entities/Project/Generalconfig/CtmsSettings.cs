using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Generalconfig
{
    public class CtmsSettings : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public bool IsCtms { get; set; }
        public Master.Project Project { get; set; }
    }
}
