using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class MProductForm : BaseEntity, ICommonAduit
    {
        public string FormName { get; set; }
        public int? CompanyId { get; set; }
    }
}