using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.Master
{
    public class DesignTrialDto : BaseDto
    {
        //[Required(ErrorMessage = "Trial Type Code is required.")]
        public string DesignTrialCode { get; set; }

        [Required(ErrorMessage = "Trial Type is required.")]
        public int TrialTypeId { get; set; }

        [Required(ErrorMessage = "Design Trial Name is required.")]
        public string DesignTrialName { get; set; }

        public string Notes { get; set; }

        public TrialType TrialType { get; set; }
        public int? CompanyId { get; set; }
    }

    public class DesignTrialGridDto : BaseAuditDto
    {
        public string DesignTrialCode { get; set; }
        public int TrialTypeId { get; set; }
        public string DesignTrialName { get; set; }
        public string Notes { get; set; }
        public TrialType TrialType { get; set; }
    }
}