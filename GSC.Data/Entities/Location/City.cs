using GSC.Common.Base;

namespace GSC.Data.Entities.Location
{
    public class City : BaseEntity
    {
        public string CityCode { get; set; }
        public string CityName { get; set; }

        public int StateId { get; set; }

        public State State { get; set; }
        public int? CompanyId { get; set; }
    }
}