using GSC.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
    public class ProcedureDto : BaseDto
    {
        [Required(ErrorMessage = "Name is Required.")]
        public string Name { get; set; }
        public int? UnitId { get; set; }
        public decimal? CostPerUnit { get; set; }
        public int? CurrencyId { get; set; }
    }

    public class ProcedureGridDto : BaseAuditDto
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal? CostPerUnit { get; set; }
        public string CurrencyType { get; set; }
    }
    public class DropDownProcedureDto
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string CurrencyType { get; set; }
        public string CurrencySymbol { get; set; }
        public decimal? CostPerUnit { get; set; }
    }
}
