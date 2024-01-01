using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.CTMS
{
    public class CurrencyRate : BaseEntity, ICommonAduit
    {      
        public int StudyPlanId { get; set; }
        public int? LocalCurrencyId { get; set; }
        public decimal? LocalCurrencyRate { get; set; }
        public int? GlobalCurrencyId { get; set; }

    }
}
