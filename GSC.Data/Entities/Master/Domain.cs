using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class Domain : BaseEntity, ICommonAduit
    {
        public string DomainCode { get; set; }

        public string DomainName { get; set; }
        public int? CompanyId { get; set; }
        public int DomainClassId { get; set; }
        public DomainClass DomainClass { get; set; }
        public bool? IsStatic { get; set; }
      //  public bool SystemType { get; set; }
    }
}