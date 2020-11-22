using GSC.Common.Base;

namespace GSC.Data.Entities.Location
{
    public class State : BaseEntity
    {
        public string StateName { get; set; }

        public int CountryId { get; set; }

        public Country Country { get; set; }
        public int? CompanyId { get; set; }
    }
}