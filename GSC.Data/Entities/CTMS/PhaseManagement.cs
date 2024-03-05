using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.CTMS
{
    public class PhaseManagement : BaseEntity, ICommonAduit
    {
        public string PhaseName { get; set; }
        public string PhaseCode { get; set; }

    }
}
