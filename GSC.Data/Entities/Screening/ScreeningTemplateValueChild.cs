using GSC.Common.Base;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateValueChild : BaseEntity
    {
        public int ScreeningTemplateValueId { get; set; }
        public int ProjectDesignVariableValueId { get; set; }
        public string Value { get; set; }
        public short? LevelNo { get; set; }
        public ProjectDesignVariableValue ProjectDesignVariableValue { get; set; }
        public ScreeningTemplateValue ScreeningTemplateValue { get; set; }
    }
}