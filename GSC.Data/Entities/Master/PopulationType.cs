using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class PopulationType : BaseEntity, ICommonAduit
    {
        public string PopulationName { get; set; }

        public int? CompanyId { get; set; }
    }
}