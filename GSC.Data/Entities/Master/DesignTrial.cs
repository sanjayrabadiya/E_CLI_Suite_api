using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class DesignTrial : BaseEntity
    {
        public string DesignTrialCode { get; set; }
        public int TrialTypeId { get; set; }

        public string DesignTrialName { get; set; }

        public string Notes { get; set; }

        public int? CompanyId { get; set; }

        public TrialType TrialType { get; set; }
    }
}