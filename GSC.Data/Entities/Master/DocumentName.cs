using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Shared.Generic;

namespace GSC.Data.Entities.Master
{
    public class DocumentName : BaseEntity, ICommonAduit
    {
        public int DocumentTypeId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public int? CompanyId { get; set; }
        public int? DocumentSize { get; set; }
        public DocumentPickFromType? PickFromType { get; set; }

        public DocumentType DocumentType { get; set; }
    }
}