using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class HolidayDto : BaseEntity
    {
        public int InvestigatorContactId { get; set; }
        [Required(ErrorMessage = "Holiday Type is required.")]
        public HolidayType HolidayType { get; set; }
        [Required(ErrorMessage = "Holiday Name is required.")]
        public string HolidayName { get; set; }
        [Required(ErrorMessage = "Holiday Date is required.")]
        public DateTime HolidayDate { get; set; }
        public string Description { get; set; }
        public int? CompanyId { get; set; }
        public string HolidayTypeName { get; set; }
    }
}
