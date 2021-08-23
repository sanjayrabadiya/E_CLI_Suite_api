using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementConfiguration : BaseEntity, ICommonAduit
    {
        public int AppScreenId { get; set; }
        public int VariableTemplateId { get; set; }
        public AppScreen AppScreen { get; set; }
        public VariableTemplate VariableTemplate { get; set; }
    }
}
