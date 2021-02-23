using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
   public class ResourceType: BaseEntity, ICommonAduit
    {
        public string ResourceName { get; set; }
        public string ResourceCode { get; set; }
    }
}
