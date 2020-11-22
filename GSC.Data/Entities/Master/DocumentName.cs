using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class DocumentName : BaseEntity
    {
        public int DocumentTypeId { get; set; }

        public string Name { get; set; }

        public int? CompanyId { get; set; }

        public DocumentType DocumentType { get; set; }
    }
}