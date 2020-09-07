using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class ReligionDto : BaseDto
    {
        [Required(ErrorMessage = "Religion Name is required.")]
        public string ReligionName { get; set; }
        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CompanyId { get; set; }
    }

    public class ReligionGridDto : BaseAuditDto
    {
        public string ReligionName { get; set; }
    }
}