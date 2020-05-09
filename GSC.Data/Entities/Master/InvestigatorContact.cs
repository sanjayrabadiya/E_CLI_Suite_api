using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;

namespace GSC.Data.Entities.Master
{
    public class InvestigatorContact : BaseEntity
    {
        public string NameOfInvestigator { get; set; }
        public string Specialization { get; set; }
        public string RegistrationNumber { get; set; }
        public string HospitalName { get; set; }
        public string HospitalAddress { get; set; }
        public string ContactNumber { get; set; }
        public string IECIRBName { get; set; }
        public int CityId { get; set; }
        public int? CompanyId { get; set; }
        public City City { get; set; }
    }
}