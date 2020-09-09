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
        public int? CompanyId { get; set; }
    }

    public class AuditReasonGridDto : BaseAuditDto
    {
        public string ReasonName { get; set; }
        public bool IsOther { get; set; }
        public string Notes { get; set; }
        public AuditModule ModuleId { get; set; }
        public string ModuleName { get; set; }

    }
}