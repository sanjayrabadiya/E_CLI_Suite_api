using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class ClientType : BaseEntity
    {
        public string ClientTypeName { get; set; }
        public int? CompanyId { get; set; }
    }
}