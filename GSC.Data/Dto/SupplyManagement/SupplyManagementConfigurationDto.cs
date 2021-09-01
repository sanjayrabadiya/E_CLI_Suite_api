using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementConfigurationDto : BaseDto
    {
        public int AppScreenId { get; set; }
        public int VariableTemplateId { get; set; }
        public AppScreen AppScreen { get; set; }
        public VariableTemplate VariableTemplate { get; set; }
    }

    public class SupplyManagementConfigurationGridDto : BaseAuditDto
    {
        public string PageName { get; set; }
        public string TemplateName { get; set; }
    }
}
