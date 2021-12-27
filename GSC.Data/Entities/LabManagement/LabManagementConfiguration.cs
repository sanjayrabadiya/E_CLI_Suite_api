using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.LabManagement
{
    public class LabManagementConfiguration : BaseEntity, ICommonAduit
    {
        public int ProjectDesignTemplateId { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public int SecurityRoleId { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public SecurityRole SecurityRole { get; set; }
    }
}
