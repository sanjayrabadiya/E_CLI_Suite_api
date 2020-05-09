using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class DomainClass : BaseEntity
    {
        public string DomainClassCode { get; set; }

        public string DomainClassName { get; set; }
        public int? CompanyId { get; set; }

        public bool? IsStatic { get; set; }
    }
}