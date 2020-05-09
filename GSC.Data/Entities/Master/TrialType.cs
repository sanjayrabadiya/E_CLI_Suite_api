using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class TrialType : BaseEntity
    {
        public string TrialTypeName { get; set; }

        public string Notes { get; set; }

        public int? CompanyId { get; set; }
    }
}