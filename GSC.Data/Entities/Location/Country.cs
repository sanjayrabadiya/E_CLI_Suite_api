using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Location
{
    public class Country : BaseEntity, ICommonAduit
    {
        public string CountryName { get; set; }

        public string CountryCallingCode { get; set; }

        public string CountryCode { get; set; }
        public int? CompanyId { get; set; }
    }
}