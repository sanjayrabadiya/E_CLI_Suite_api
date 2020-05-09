using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class ScopeName : BaseEntity
    {
        public string Name { get; set; }

        public string Notes { get; set; }
        public int? CompanyId { get; set; }
    }
}