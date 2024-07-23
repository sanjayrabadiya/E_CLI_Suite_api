using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class ContractTemplateFormat : BaseEntity, ICommonAduit
    {
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string TemplateFormat { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
