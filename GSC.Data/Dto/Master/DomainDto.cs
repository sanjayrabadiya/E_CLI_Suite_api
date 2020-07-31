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

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        //public int CreatedBy { get; set; }
        //public int? DeletedBy { get; set; }
        //public int? ModifiedBy { get; set; }
        //public DateTime? CreatedDate { get; set; }
        //public DateTime? ModifiedDate { get; set; }
        //public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        //public string CompanyName { get; set; }
    }
}