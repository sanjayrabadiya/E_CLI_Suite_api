using GSC.Common.Base;
using GSC.Data.Entities.Location;

namespace GSC.Data.Entities.Master
{
    public class InvestigatorContact : BaseEntity
    {
        public string NameOfInvestigator { get; set; }
        public string EmailOfInvestigator { get; set; }
        public string RegistrationNumber { get; set; }
        public int? TrialTypeId { get; set; }
        public string ContactNumber { get; set; }
        public int? CompanyId { get; set; }
        public TrialType TrialType { get; set; }
    }
}