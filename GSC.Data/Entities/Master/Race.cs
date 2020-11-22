using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class Race : BaseEntity
    {
        public string RaceName { get; set; }
        public int? CompanyId { get; set; }
    }
}