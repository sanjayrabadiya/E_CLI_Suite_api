using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
   public class Iecirb : BaseEntity
    {
        public int? ManageSiteId { get; set; }
        public string IECIRBName { get; set; }
        public string RegistrationNumber { get; set; }
        public string IECIRBContactName { get; set; }
        public string IECIRBContactEmail { get; set; }
        public string IECIRBContactNumber { get; set; }
        public int? CompanyId { get; set; }
        public ManageSite ManageSite { get; set; }
    }
}
