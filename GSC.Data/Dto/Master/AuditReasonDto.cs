using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class AuditReasonDto : BaseDto
    {
        [Required(ErrorMessage = "Reason Name is required.")]
        public string ReasonName { get; set; }

        [Required(ErrorMessage = "Module Name is required.")]
        public AuditModule ModuleId { get; set; }

        public bool IsOther { get; set; }
        public string Notes { get; set; }
        public string ModuleName { get; set; }
        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}