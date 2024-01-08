using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System;


namespace GSC.Data.Entities.CTMS
{
   public class Procedure : BaseEntity, ICommonAduit
    {
        public string Name { get; set; }    
        public int? UnitId { get; set; }
        public int? CostPerUnit { get; set; }    
        public int? CurrencyId { get; set; }
        public Unit Unit { get; set; }
        public Currency Currency { get; set; }
    }
}
