using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class DocumentName : BaseEntity, ICommonAduit
    {
        public int DocumentTypeId { get; set; }

        public string Name { get; set; }

        public int? CompanyId { get; set; }

        public DocumentType DocumentType { get; set; }
    }
}