using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class UnitDto : BaseDto
    {
        [Required(ErrorMessage = "Unit Name is required.")]
        public string UnitName { get; set; }
        public int? CompanyId { get; set; }
    }

    public class UnitGridDto : BaseAuditDto
    {
        public string UnitName { get; set; }
    }
}