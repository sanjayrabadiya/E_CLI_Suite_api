using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.CTMS
{

    public class PatientCostGridDto : BaseDto
    {
        public int? ProjectId { get; set; }
        public int? ProcedureId { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public int? Cost { get; set; }
        public int? TotalProcedure { get; set; }
        public int? TotalProjectDesignVisit { get; set; }
        public int? Total { get; set; }

    }
    public class ProcedureVisitdadaDto : BaseAuditDto
    {
        public string Name { get; set; }
        public int? UnitId { get; set; }
        public int? CostPerUnit { get; set; }
        public List<VisitdadaDto> VisitdadaDto { get; set; }
    }
    public class VisitdadaDto
    {
        public int Id { get; set; }
        public string VisitName { get; set; }
        public int? Cost { get; set; }
        public int?  Total { get; set; }
    }
}
