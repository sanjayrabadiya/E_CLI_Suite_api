using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Entities.LabManagement
{
    public class LabManagementVariableMapping : BaseEntity, ICommonAduit
    {
        public int LabManagementConfigurationId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string TargetVariable { get; set; }
        public LabManagementConfiguration LabManagementConfiguration { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
    }
}
