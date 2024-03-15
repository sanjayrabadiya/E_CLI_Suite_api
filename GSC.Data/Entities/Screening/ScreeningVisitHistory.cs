using GSC.Common.Base;
using GSC.Helper;
using System;

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
