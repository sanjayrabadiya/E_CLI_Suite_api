using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class TrialType : BaseEntity, ICommonAduit
    {
        public string TrialTypeName { get; set; }

        public string Notes { get; set; }

        public int? CompanyId { get; set; }
    }
}