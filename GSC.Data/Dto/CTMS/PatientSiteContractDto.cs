using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.Common;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class PatientSiteContractDto : BaseDto
    {

        [Required(ErrorMessage = "SiteContractId is required.")]
        public int SiteContractId { get; set; }

        [Required(ErrorMessage = "Visit is required.")]
        public int ProjectDesignVisitId { get; set; }

        [Required(ErrorMessage = "Visit Total is required.")]
        public decimal VisitTotal { get; set; }

        [Required(ErrorMessage = "PayableTotal Total is required.")]
        public decimal PayableTotal { get; set; }

    }
    public class PatientSiteContractGridDto : BaseAuditDto
    {
        public string ContractName { get; set; }
        public string VisitName { get; set; }
        public decimal PassThroughTotalRate { get; set; }
        public decimal PayableTotal { get; set; }
        public string ContractCode { get; set; }
    }
}
