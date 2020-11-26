using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class ClientType : BaseEntity, ICommonAduit
    {
        public string ClientTypeName { get; set; }
        public int? CompanyId { get; set; }
    }
}