using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
  public  class ProcedureDto : BaseDto
    {
        [Required(ErrorMessage = "Name is Required.")]
        public string Name {  get; set; }
        public int? UnitId { get; set; }
        public int? CostPerUnit { get; set; }
        public int? CurrencyId { get; set; }
    }

    public class ProcedureGridDto : BaseAuditDto
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public int? CostPerUnit { get; set; }
        public string CurrencyType { get; set; }
    }
}
