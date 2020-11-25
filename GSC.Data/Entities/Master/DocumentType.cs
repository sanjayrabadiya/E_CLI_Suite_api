using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class DocumentType : BaseEntity, ICommonAduit
    {
        public string TypeName { get; set; }

        public string Note { get; set; }
        public int? CompanyId { get; set; }
    }
}