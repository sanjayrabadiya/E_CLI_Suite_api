using GSC.Data.Entities.Common;
using GSC.Data.Entities.LabManagement;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Data.Dto.LabManagement
{
    public class LabManagementVariableMappingDto : BaseDto
    {
        public int LabManagementConfigurationId { get; set; }
        public IList<LabManagementVariableMappingDetail> LabManagementVariableMappingDetail { get; set; }
        public LabManagementConfiguration LabManagementConfiguration { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
    }

    public class LabManagementVariableMappingDetail
    {
        public int ProjectDesignVariableId { get; set; }
        public string TargetVariable { get; set; }
    }
}
