using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class MaritalStatus : BaseEntity
    {
        public string MaritalStatusName { get; set; }
        public int? CompanyId { get; set; }
    }
}