using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Data.Entities.LabManagement
{
    public class LabManagementConfiguration : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public Entities.Master.Project Project { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public IList<LabManagementVariableMapping> LabManagementVariableMapping { get; set; }
    }
}
