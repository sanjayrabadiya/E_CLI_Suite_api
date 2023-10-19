using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;

namespace GSC.Data.Entities.Master
{
    public class Currency : BaseEntity, ICommonAduit
    {
        public string CurrencyName { get; set; }
        public int CountryId { get; set; }
        public string CurrencySymbol { get; set; }     
        public Country Country { get; set; }
    }
}
