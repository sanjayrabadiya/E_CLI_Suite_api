using GSC.Common.Base;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVariableValue : BaseEntity
    {
        public int ProjectDesignVariableId { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
        public int SeqNo { get; set; }
    }
}