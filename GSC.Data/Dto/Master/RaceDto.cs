using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class RaceDto : BaseDto
    {
        [Required(ErrorMessage = "Race Name is required.")]
        public string RaceName { get; set; }
        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CompanyId { get; set; }
    }

    public class RaceGridDto : BaseAuditDto
    {
        public string RaceName { get; set; }
    }
}