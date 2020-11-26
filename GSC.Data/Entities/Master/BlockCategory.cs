using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class BlockCategory : BaseEntity, ICommonAduit
    {
        public string BlockCode { get; set; }
        public string BlockCategoryName { get; set; }
        public int? CompanyId { get; set; }
    }
}