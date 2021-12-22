using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.Master
{
    public class DomainDto : BaseDto
    {
        [Required(ErrorMessage = "Domain Code is required.")]
        public string DomainCode { get; set; }

        [Required(ErrorMessage = "Domain Name is required.")]
        public string DomainName { get; set; }

        [Required(ErrorMessage = "Domain Class is required.")]
        public int DomainClassId { get; set; }

        public string DomainClassName { get; set; }

        public DomainClass DomainClass { get; set; }

        public bool? IsStatic { get; set; }

        public int? CompanyId { get; set; }
        public bool SystemType { get; set; }

    }


    public class DomainGridDto : BaseAuditDto
    {
        public string DomainCode { get; set; }
        public string DomainName { get; set; }
        public string DomainClassName { get; set; }
        public bool SystemType { get; set; }

    }
}