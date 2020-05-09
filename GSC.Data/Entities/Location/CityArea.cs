using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Location
{
    public class CityArea : BaseEntity
    {
        public string AreaName { get; set; }
        public int CityId { get; set; }
        public int? CompanyId { get; set; }
        public City City { get; set; }
    }
}