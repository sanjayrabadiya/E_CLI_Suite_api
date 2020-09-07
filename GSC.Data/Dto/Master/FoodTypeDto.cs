using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class FoodTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Food type is required.")]
        public string TypeName { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CompanyId { get; set; }
    }

    public class FoodTypeGridDto : BaseAuditDto
    {
        public string TypeName { get; set; }
    }
}