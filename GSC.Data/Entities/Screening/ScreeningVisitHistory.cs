using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningVisitHistory : BaseEntity
    {
        public int RoleId { get; set; }
        public int ScreeningVisitId { get; set; }
        public DateTime? StatusDate { get; set; }
        public ScreeningVisitStatus VisitStatusId { get; set; }
        public string Notes { get; set; }
        public ScreeningVisit ScreeningVisit { get; set; }
    }
}
