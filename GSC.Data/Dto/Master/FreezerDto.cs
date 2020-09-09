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
        public int? CompanyId { get; set; }
    }

    public class FreezerGridDto : BaseAuditDto
    {
        public string FreezerName { get; set; }
        public string FreezerTypeName { get; set; }
        public FreezerType FreezerType { get; set; }
        public string Location { get; set; }
        public string Temprature { get; set; }
        public int Capacity { get; set; }
        public string Note { get; set; }
    }
}
