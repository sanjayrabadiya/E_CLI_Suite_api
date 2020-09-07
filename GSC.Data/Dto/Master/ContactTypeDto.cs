using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class ContactTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Contact Code is required.")]
        public string ContactCode { get; set; }

        [Required(ErrorMessage = "Contact Type Name is required.")]
        public string TypeName { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CompanyId { get; set; }
    }

    public class ContactTypeGridDto : BaseAuditDto
    {
        public string ContactCode { get; set; }
        public string TypeName { get; set; }
    }
}