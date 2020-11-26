using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class ContactType : BaseEntity, ICommonAduit
    {
        public string ContactCode { get; set; }

        public string TypeName { get; set; }
        public int? CompanyId { get; set; }
    }
}