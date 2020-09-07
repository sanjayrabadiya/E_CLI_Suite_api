using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class PopulationTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Population Name is required.")]
        public string PopulationName { get; set; }
        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CompanyId { get; set; }
    }

    public class PopulationTypeGridDto : BaseAuditDto
    {
        public string PopulationName { get; set; }
    }
}