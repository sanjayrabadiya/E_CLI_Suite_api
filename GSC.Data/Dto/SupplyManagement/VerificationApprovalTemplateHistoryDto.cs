using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class VerificationApprovalTemplateHistoryDto:BaseDto
    {
        public int VerificationApprovalTemplateId { get; set; }
        public int SendBy { get; set; }
        public DateTime SendOn { get; set; }
        public bool IsSendBack { get; set; }
    }
}
