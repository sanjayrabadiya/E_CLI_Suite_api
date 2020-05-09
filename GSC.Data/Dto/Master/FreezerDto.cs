using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class FreezerDto : BaseDto
    {
        [Required(ErrorMessage = "Freezer Name is required.")]
        public string FreezerName { get; set; }

        [Required(ErrorMessage = "Freezer Type is required.")]
        public FreezerType FreezerType { get; set; }

        public string FreezerTypeName { get; set; }

        [Required(ErrorMessage = "Freezer Location is required.")]
        public string Location { get; set; }

        public string Temprature { get; set; }

        public int Capacity { get; set; }

        public string Note { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}
