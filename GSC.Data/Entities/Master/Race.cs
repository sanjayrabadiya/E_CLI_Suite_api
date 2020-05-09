using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class Race : BaseEntity
    {
        public string RaceName { get; set; }
        public int? CompanyId { get; set; }
    }
}