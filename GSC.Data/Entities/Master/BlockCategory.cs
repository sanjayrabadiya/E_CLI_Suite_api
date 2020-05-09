using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class BlockCategory : BaseEntity
    {
        public string BlockCode { get; set; }
        public string BlockCategoryName { get; set; }
        public int? CompanyId { get; set; }
    }
}