using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class TestGroup : BaseEntity, ICommonAduit
    {
        public string TestGroupName { get; set; }

        public string Notes { get; set; }

        public int? CompanyId { get; set; }
    }
}