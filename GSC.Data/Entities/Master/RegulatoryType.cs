using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class RegulatoryType : BaseEntity, ICommonAduit
    {
        public string RegulatoryTypeCode { get; set; }
        public string RegulatoryTypeName { get; set; }
        public int? CompanyId { get; set; }
    }
}
