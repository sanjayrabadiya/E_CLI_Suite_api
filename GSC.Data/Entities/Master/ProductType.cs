using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class ProductType : BaseEntity, ICommonAduit
    {
        public string ProductTypeCode { get; set; }

        public string ProductTypeName { get; set; }

        public int? CompanyId { get; set; }
    }
}