using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class TrialTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Trial Type Name is required.")]
        public string TrialTypeName { get; set; }

        public string Notes { get; set; }

        public int? CompanyId { get; set; }
    }

    public class TrialTypeGridDto : BaseAuditDto
    {
        public string TrialTypeName { get; set; }
        public string Notes { get; set; }
    }
}