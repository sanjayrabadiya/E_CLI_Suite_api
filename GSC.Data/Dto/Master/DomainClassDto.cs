using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class DomainClassDto : BaseDto
    {
        [Required(ErrorMessage = "Domain Class Code is required.")]
        public string DomainClassCode { get; set; }

        [Required(ErrorMessage = "Domain Class Name is required.")]
        public string DomainClassName { get; set; }

        public bool? IsStatic { get; set; }

        public int? CompanyId { get; set; }
        public bool SystemType { get; set; }

    }

    public class DomainClassGridDto : BaseAuditDto
    {
        public string DomainClassCode { get; set; }
        public string DomainClassName { get; set; }
        public bool SystemType { get; set; }
    }
}