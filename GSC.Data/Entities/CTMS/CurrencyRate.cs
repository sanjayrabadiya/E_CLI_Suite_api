using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.CTMS
{
    public class CurrencyRate : BaseEntity, ICommonAduit
    {      
        public int StudyPlanId { get; set; }
        public int? CurrencyId { get; set; } //LocalCurrencyId
        public decimal? LocalCurrencyRate { get; set; }
        public int? GlobalCurrencyId { get; set; }
        public Currency Currency { get; set; }

    }
}
