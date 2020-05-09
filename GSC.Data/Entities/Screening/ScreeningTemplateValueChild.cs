using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateValueChild : BaseEntity
    {
        public int ScreeningTemplateValueId { get; set; }
        public int ProjectDesignVariableValueId { get; set; }
        public string Value { get; set; }
        public ProjectDesignVariableValue ProjectDesignVariableValue { get; set; }
    }
}