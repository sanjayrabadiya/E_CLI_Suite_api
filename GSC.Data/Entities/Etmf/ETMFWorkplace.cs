using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ETMFWorkplace : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
    }
}
