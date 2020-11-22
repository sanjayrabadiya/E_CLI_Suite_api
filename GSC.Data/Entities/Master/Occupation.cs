using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class Occupation : BaseEntity
    {
        public string OccupationName { get; set; }
        public int? CompanyId { get; set; }
    }
}