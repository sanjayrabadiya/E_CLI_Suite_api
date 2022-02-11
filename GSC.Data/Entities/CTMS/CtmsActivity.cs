using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsActivity : BaseEntity, ICommonAduit
    {
        public string ActivityCode { get; set; }
        public string ActivityName { get; set; }
    }
}
