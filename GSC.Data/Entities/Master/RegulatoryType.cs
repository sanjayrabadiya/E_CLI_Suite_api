using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class RegulatoryType : BaseEntity
    {
        public string RegulatoryTypeCode { get; set; }
        public string RegulatoryTypeName { get; set; }
        public int? CompanyId { get; set; }
    }
}
