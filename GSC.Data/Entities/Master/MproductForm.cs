using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class MProductForm : BaseEntity
    {
        public string FormName { get; set; }
        public int? CompanyId { get; set; }
    }
}