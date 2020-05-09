using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class Occupation : BaseEntity
    {
        public string OccupationName { get; set; }
        public int? CompanyId { get; set; }
    }
}