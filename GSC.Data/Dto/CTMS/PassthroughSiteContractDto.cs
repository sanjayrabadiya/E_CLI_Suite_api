using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.Common;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class PassthroughSiteContractDto : BaseDto
    {
        [Required(ErrorMessage = "SiteContractId is required.")]
        public int SiteContractId { get; set; }

        [Required(ErrorMessage = "Visit is required.")]
        public int PassThroughCostActivityId { get; set; }

        [Required(ErrorMessage = "Visit Total is required.")]
        public decimal PassThroughTotalRate { get; set; }

        [Required(ErrorMessage = "PayableTotal Total is required.")]
        public decimal PayableTotal { get; set; }

    }
    public class PassthroughSiteContractGridDto : BaseAuditDto
    {
        public string ContractName { get; set; }
        public string ActivityName { get; set; }
        public decimal PassThroughTotalRate { get; set; }
        public decimal PayableTotal { get; set; }
        public string ContractCode { get; set; }
    }
}
