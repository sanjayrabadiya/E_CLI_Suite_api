using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
namespace GSC.Data.Entities.CTMS
{
    public class SiteContract : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int SiteId { get; set; }
        public int? CountryId { get; set; }
        public string ContractFileName { get; set; }
        public string ContractCode { get; set; }
        public string ContractDocumentPath { get; set; }
        public string Remark { get; set; }

        public Master.Project Project { get; set; }
        public Country Country { get; set; }
    }
}
