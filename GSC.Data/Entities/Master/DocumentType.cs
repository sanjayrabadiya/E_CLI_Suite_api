using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class DocumentType : BaseEntity
    {
        public string TypeName { get; set; }

        public string Note { get; set; }
        public int? CompanyId { get; set; }
    }
}