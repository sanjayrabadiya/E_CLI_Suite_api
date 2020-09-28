using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class DrugDto : BaseDto
    {
        [Required(ErrorMessage = "Drug Name is required.")]
        public string DrugName { get; set; }
        public string Strength { get; set; }
        public string DosageForm { get; set; }
        public int? CompanyId { get; set; }
    }

    public class DrugGridDto : BaseAuditDto
    {
        public string DrugName { get; set; }
        public string Strength { get; set; }
        public string DosageForm { get; set; }
    }
}