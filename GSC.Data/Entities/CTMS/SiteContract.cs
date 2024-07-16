using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
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
        public bool? IsDocument { get; set; }
        public int? ContractTemplateFormatId { get; set; }
        public string FormatBody { get; set; }

        public Master.Project Project { get; set; }
        public Country Country { get; set; }
        public ContractTemplateFormat ContractTemplateFormat { get; set; }
        public bool? IsApproved { get; set; }
    }
}
