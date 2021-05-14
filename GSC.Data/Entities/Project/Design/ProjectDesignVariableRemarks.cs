using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVariableRemarks : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVariableId { get; set; }
        public int Range { get; set; }
        public string Remarks { get; set; }
        public int SeqNo { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
    }
}
