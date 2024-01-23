using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Shared.Extension;
using System;
namespace GSC.Data.Entities.CTMS
{
    public class PatientCost : BaseEntity, ICommonAduit
    {
       
        public int? ProjectId { get; set; }
        public int? ProcedureId { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public int? Cost { get; set; }
        public int? TotalProcedure { get; set; }
        public int? TotalProjectDesignVisit { get; set; }
        public int? Total { get; set; }

        public Master.Project Project { get; set; }
        public Procedure Procedure { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
    }
}
