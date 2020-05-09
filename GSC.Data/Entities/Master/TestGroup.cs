using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class TestGroup : BaseEntity
    {
        public string TestGroupName { get; set; }

        public string Notes { get; set; }

        public int? CompanyId { get; set; }
    }
}