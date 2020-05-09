using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class Product : BaseEntity
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int ProjectId { get; set; }
        public int ProductTypeId { get; set; }
        public int? CompanyId { get; set; }
        public ProductType ProductType { get; set; }
        public Project Project { get; set; }
    }
}