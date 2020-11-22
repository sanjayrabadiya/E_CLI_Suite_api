using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class MaritalStatus : BaseEntity
    {
        public string MaritalStatusName { get; set; }
        public int? CompanyId { get; set; }
    }
}