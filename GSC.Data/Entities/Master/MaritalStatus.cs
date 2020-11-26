using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class MaritalStatus : BaseEntity, ICommonAduit
    {
        public string MaritalStatusName { get; set; }
        public int? CompanyId { get; set; }
    }
}