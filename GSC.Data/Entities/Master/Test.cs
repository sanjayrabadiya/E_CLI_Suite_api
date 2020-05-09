using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class Test : BaseEntity
    {
        public string TestName { get; set; }

        public int TestGroupId { get; set; }

        public string Anticoagulant { get; set; }

        public string Notes { get; set; }

        public int? CompanyId { get; set; }

        public TestGroup TestGroup { get; set; }
    }
}