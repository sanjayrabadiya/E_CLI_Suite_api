using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class Occupation : BaseEntity, ICommonAduit
    {
        public string OccupationName { get; set; }
        public int? CompanyId { get; set; }
    }
}