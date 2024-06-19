using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Data.Entities.Master
{
    public class PaymentTerms: BaseEntity, ICommonAduit
    {
        public int Terms { get; set; }
    }
}
