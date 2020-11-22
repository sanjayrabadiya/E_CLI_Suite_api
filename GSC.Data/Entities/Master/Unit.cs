using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class Unit : BaseEntity
    {
        public string UnitName { get; set; }
        public int? CompanyId { get; set; }
    }
}