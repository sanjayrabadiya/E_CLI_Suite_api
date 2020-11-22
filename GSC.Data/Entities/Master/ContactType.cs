using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class ContactType : BaseEntity
    {
        public string ContactCode { get; set; }

        public string TypeName { get; set; }
        public int? CompanyId { get; set; }
    }
}