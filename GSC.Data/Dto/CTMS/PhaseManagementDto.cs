using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class PhaseManagementDto : BaseDto
    {
        [Required(ErrorMessage = "Phase Name is required.")]
        public string PhaseName { get; set; }
        [Required(ErrorMessage = "Phase Code is required.")]
        public string PhaseCode { get; set; }
    }

    public class PhaseManagementGridDto: BaseAuditDto
    {
        public string PhaseName { get; set; }
        public string PhaseCode { get; set; }
    }

}
