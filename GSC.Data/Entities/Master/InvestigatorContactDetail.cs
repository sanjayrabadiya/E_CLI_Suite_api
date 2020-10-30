using GSC.Data.Entities.Common;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.Master
{
    public class InvestigatorContactDetail : BaseEntity
    {
        public int? InvestigatorContactId { get; set; }
        public int? ContactTypeId { get; set; }
        //public int? SecurityRoleId { get; set; }
        public string ContactEmail { get; set; }
        public string ContactName { get; set; }
        public string ContactNo { get; set; }
        public int? CompanyId { get; set; }
        //public SecurityRole SecurityRole { get; set; }
        public ContactType ContactType { get; set; }
    }
}
