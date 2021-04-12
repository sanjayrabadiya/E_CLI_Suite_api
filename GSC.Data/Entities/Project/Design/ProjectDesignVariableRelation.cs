using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVariableRelation : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVariableId { get; set; }
        public int ProjectDesignRelationVariableId { get; set; }
       // public int ProjectDesignSuggestionVariableId { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
    }
}
