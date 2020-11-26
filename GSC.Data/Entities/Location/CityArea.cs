using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Location
{
    public class CityArea : BaseEntity, ICommonAduit
    {
        public string AreaName { get; set; }
        public int CityId { get; set; }
        public int? CompanyId { get; set; }
        public City City { get; set; }
    }
}