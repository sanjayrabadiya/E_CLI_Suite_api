using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Project.Design
{
    public class WorkflowTemplate : BaseEntity, ICommonAduit
    {
        public int LevelNo { get; set; }
        public int ProjectDesignTemplateId { get; set; }
    }
}
