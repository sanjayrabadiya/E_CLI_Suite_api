using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class ClientTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Client Type Name is required.")]
        public string ClientTypeName { get; set; }
        public int? CompanyId { get; set; }
    }

    public class ClientTypeGridDto : BaseAuditDto
    {
        public string ClientTypeName { get; set; }
    }
}