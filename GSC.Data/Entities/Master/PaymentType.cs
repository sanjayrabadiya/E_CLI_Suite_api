using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Data.Entities.Master
{
    public class PaymentType: BaseEntity, ICommonAduit
    {
        public string Name { get; set; }
    }
}
