using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
   public class VisitStatusDto : BaseDto
    {
        [Required(ErrorMessage = "Code is required.")]
        public Int16 Code { get; set; }
        [Required(ErrorMessage = "Status Name is required.")]
        public string StatusName { get; set; }
        [Required(ErrorMessage = "Display Name is required.")]
        public string DisplayName { get; set; }
        public int? CompanyId { get; set; }
    }
    public class VisitStatusGridDto : BaseAuditDto
    {
        public Int16 Code { get; set; }
        public string StatusName { get; set; }
        public string DisplayName { get; set; }
        public int? CompanyId { get; set; }
    }
}
