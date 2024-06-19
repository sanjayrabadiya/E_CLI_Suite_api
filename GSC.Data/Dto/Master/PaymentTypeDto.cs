using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Data.Dto.Master
{
    public class PaymentTypeDto : BaseDto
    {
        public string Name { get; set; }
    }

    public class PaymentTypeGridDto : BaseAuditDto
    {
        public string Name { get; set; }
    }
}
