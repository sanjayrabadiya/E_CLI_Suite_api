using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningSetting : BaseEntity, ICommonAduit
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int VisitId { get; set; }
        public int RoleId { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
    }
}
