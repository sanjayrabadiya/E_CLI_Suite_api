using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.Configuration
{
    public class PharmacyConfig : BaseEntity, ICommonAduit
    {
        public int FormId { get; set; }
        public string FormName { get; set; }
        public int VariableTemplateId { get; set; }
        public int? CompanyId { get; set; }
        public VariableTemplate VariableTemplate { get; set; }
    }
}