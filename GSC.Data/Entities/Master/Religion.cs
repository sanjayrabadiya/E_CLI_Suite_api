using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class Religion : BaseEntity, ICommonAduit
    {
        public string ReligionName { get; set; }
        public int? CompanyId { get; set; }
    }
}