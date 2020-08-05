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

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        //public int CreatedBy { get; set; }
        //public int? DeletedBy { get; set; }
        //public int? ModifiedBy { get; set; }
        //public DateTime? CreatedDate { get; set; }
        //public DateTime? ModifiedDate { get; set; }
        //public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        //public string CompanyName { get; set; }
    }

    public class TrialTypeGridDto : BaseAuditDto
    {
        public string TrialTypeName { get; set; }
        public string Notes { get; set; }
    }
}