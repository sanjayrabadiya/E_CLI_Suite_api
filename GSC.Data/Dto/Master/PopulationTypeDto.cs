using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class PopulationTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Population Name is required.")]
        public string PopulationName { get; set; }
        public int? CompanyId { get; set; }
    }

    public class PopulationTypeGridDto : BaseAuditDto
    {
        public string PopulationName { get; set; }
    }
}