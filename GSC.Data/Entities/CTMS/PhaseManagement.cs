using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
   public class PhaseManagement: BaseEntity, ICommonAduit
    {
        public string PhaseName { get; set; }
        public string PhaseCode { get; set; }
     
      
    }
}
