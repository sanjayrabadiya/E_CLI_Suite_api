using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class ScopeNameDto : BaseDto
    {
        [Required(ErrorMessage = "Scope Name is required.")]
        public string ScopeName { get; set; }
        public string Notes { get; set; }
        public int? CompanyId { get; set; }
    }

    public class ScopeNameGridDto : BaseAuditDto
    {
        public string ScopeName { get; set; }
        public string Notes { get; set; }
    }
}