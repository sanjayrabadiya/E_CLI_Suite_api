using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class OccupationDto : BaseDto
    {
        [Required(ErrorMessage = "Occupation Name is required.")]

        public string OccupationName { get; set; }
        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CompanyId { get; set; }
    }

    public class OccupationGridDto : BaseAuditDto
    {
        public string OccupationName { get; set; }
    }
}