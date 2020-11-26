using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class Race : BaseEntity, ICommonAduit
    {
        public string RaceName { get; set; }
        public int? CompanyId { get; set; }
    }
}