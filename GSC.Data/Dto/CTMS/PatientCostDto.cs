
using GSC.Data.Entities.Common;
using System.Collections.Generic;

namespace GSC.Data.Dto.CTMS
{
    public class ProcedureVisitdadaDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? ProcedureId { get; set; }
        public string ProcedureName { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public string VisitName { get; set; }
        public string VisitDescription { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Cost { get; set; }
        public decimal? FinalCost { get; set; }
        public decimal? CurrencyRate { get; set; }
        public string CurrencySymbol { get; set; }
        public string GlobleCurrencySymbol { get; set; }
        public bool IfEdit { get; set; }
        public bool IfPull { get; set; }
    }

    public class PatientCostGridData
    {
        public int? ProjectId { get; set; }
        public int? ProcedureId { get; set; }
        public string ProcedureName { get; set; }
        public string CurrencyType { get; set; }
        public decimal? Rate { get; set; }
        public decimal? CurrencyRate { get; set; }
        public string CurrencySymbol { get; set; }
        public List<VisitGridData> VisitGridDatas { get; set; }
    }
    public class VisitGridData
    {
        public int? VisitId { get; set; }
        public string VisitName { get; set; }
        public decimal? FinalCost { get; set; }
    }
}
