using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class BlockCategoryDto : BaseDto
    {
        [Required(ErrorMessage = "Block Code is required.")]
        public string BlockCode { get; set; }

        [Required(ErrorMessage = "Block Name is required.")]
        public string BlockCategoryName { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CompanyId { get; set; }
    }

    public class BlockCategoryGridDto : BaseAuditDto
    {
        public string BlockCode { get; set; }
        public string BlockCategoryName { get; set; }

    }
}