using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Data.Dto.Master
{
    public class PaymentTermsDto : BaseDto
    {
        public int Terms { get; set; }
    }

    public class PaymentTermsGridDto : BaseAuditDto
    {
        public int Terms { get; set; }
    }
}
