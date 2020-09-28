using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class RegulatoryTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Regulatory Code is required.")]
        public string RegulatoryTypeCode { get; set; }
        [Required(ErrorMessage = "Regulatory Name is required.")]
        public string RegulatoryTypeName { get; set; }
        public int? CompanyId { get; set; }
    }

    public class RegulatoryTypeGridDto : BaseAuditDto
    {
        public string RegulatoryTypeCode { get; set; }
        public string RegulatoryTypeName { get; set; }
        public int? CompanyId { get; set; }
    }
}