using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class ContactType : BaseEntity
    {
        public string ContactCode { get; set; }

        public string TypeName { get; set; }
        public int? CompanyId { get; set; }
    }
}