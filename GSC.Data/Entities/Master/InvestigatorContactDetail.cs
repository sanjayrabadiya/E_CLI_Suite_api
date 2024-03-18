using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.Master
{
    public class InvestigatorContactDetail : BaseEntity, ICommonAduit
    {
        public int? InvestigatorContactId { get; set; }
        public int? ContactTypeId { get; set; }
        public string ContactEmail { get; set; }
        public string ContactName { get; set; }
        public string ContactNo { get; set; }
        public int? CompanyId { get; set; }
        public ContactType ContactType { get; set; }
    }
}
