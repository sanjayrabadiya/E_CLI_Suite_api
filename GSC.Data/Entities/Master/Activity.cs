using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class Activity : BaseEntity
    {
        public string ActivityName { get; set; }
        public int? CompanyId { get; set; }
    }
}