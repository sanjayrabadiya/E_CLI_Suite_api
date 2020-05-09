using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class ProductTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Product Type Code is required.")]
        public string ProductTypeCode { get; set; }
        [Required(ErrorMessage = "Product Type name is required.")]
        public string ProductTypeName { get; set; }
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