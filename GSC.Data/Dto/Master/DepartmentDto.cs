using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class DepartmentDto : BaseDto
    {
        [Required(ErrorMessage = "Department Code is required.")]
        public string DepartmentCode { get; set; }

        [Required(ErrorMessage = "Department Name is required.")]
        public string DepartmentName { get; set; }
        public string Notes { get; set; }
        public int? CompanyId { get; set; }
    }

    public class DepartmentGridDto : BaseAuditDto
    {
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string Notes { get; set; }
    }
}