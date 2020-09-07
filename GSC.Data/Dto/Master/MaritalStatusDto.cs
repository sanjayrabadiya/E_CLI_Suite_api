using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class MaritalStatusDto : BaseDto
    {
        [Required(ErrorMessage = "Marital Status Name is required.")]
        public string MaritalStatusName { get; set; }
        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CompanyId { get; set; }
    }

    public class MaritalStatusGridDto : BaseAuditDto
    {
        public string MaritalStatusName { get; set; }
    }
}