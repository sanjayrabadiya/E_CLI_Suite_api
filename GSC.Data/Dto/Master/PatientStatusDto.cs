﻿using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class PatientStatusDto : BaseDto
    {
        [Required(ErrorMessage = "Code is required.")]
        public int Code { get; set; }
        [Required(ErrorMessage = "Status Name is required.")]
        public string StatusName { get; set; }
        public int? CompanyId { get; set; }
    }
    public class PatientStatusGridDto : BaseAuditDto
    {
        public int Code { get; set; }
        public string StatusName { get; set; }
        public int? CompanyId { get; set; }
    }
}
