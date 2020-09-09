using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;

namespace GSC.Data.Entities.Master
{
    public class InvestigatorContact : BaseEntity
    {
        public string NameOfInvestigator { get; set; }
        public string EmailOfInvestigator { get; set; }
        public string Specialization { get; set; }
        public string RegistrationNumber { get; set; }
        public int ManageSiteId { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public int IecirbId { get; set; }
        public string IECIRBContactNo { get; set; }
        public string IECIRBContactName { get; set; }
        public string IECIRBContactEmail { get; set; }
        public int CityId { get; set; }
        public int? CompanyId { get; set; }
        public City City { get; set; }
        public ManageSite ManageSite { get; set; }
        public Iecirb Iecirb { get; set; }
    }
}