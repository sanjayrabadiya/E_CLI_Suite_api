using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class MProductForm : BaseEntity
    {
        public string FormName { get; set; }
        public int? CompanyId { get; set; }
    }
}