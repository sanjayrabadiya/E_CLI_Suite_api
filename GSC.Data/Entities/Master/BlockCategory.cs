using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class BlockCategory : BaseEntity
    {
        public string BlockCode { get; set; }
        public string BlockCategoryName { get; set; }
        public int? CompanyId { get; set; }
    }
}