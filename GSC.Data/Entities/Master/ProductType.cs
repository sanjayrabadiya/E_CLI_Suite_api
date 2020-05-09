using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class ProductType : BaseEntity
    {
        public string ProductTypeCode { get; set; }

        public string ProductTypeName { get; set; }

        public int? CompanyId { get; set; }
    }
}