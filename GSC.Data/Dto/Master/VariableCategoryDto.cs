using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class VariableCategoryDto : BaseDto
    {
        [Required(ErrorMessage = "Category Code is required.")]
        public string CategoryCode { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        public string CategoryName { get; set; }

        public VariableCategoryType? SystemType { get; set; }
        public int? CompanyId { get; set; }
    }

    public class VariableCategoryGridDto : BaseAuditDto
    {
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public VariableCategoryType? SystemType { get; set; }
    }
}