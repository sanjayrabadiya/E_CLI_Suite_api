using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.SupplyManagement
{
    public class ProductTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Product Type Code is required.")]
        public string ProductTypeCode { get; set; }
        [Required(ErrorMessage = "Product Type name is required.")]
        public string ProductTypeName { get; set; }
        public int? CompanyId { get; set; }
    }

    public class ProductTypeGridDto : BaseAuditDto
    {
        public string ProductTypeCode { get; set; }
        public string ProductTypeName { get; set; }
    }
}