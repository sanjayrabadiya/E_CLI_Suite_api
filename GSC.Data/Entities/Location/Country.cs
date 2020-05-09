using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Location
{
    public class Country : BaseEntity
    {
        public string CountryName { get; set; }

        public string CountryCallingCode { get; set; }

        public string CountryCode { get; set; }
        public int? CompanyId { get; set; }
    }
}