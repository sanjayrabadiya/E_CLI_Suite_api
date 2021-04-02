using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignVariableRelationDto : BaseDto
    {
        public int? ProjectDesignVisitId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int ProjectDesignRelationVariableId { get; set; }
        public int? ProjectDesignSuggestionVariableId { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public int? ProjectDesignTemplateId { get; set; }

    }

    public class ProjectDesignVariableDisplayRelationDto : BaseDto
    {
        public string ProjectDesignVisitName { get; set; }
        public string ProjectDesignVariableName { get; set; }
        public string ProjectDesignRelationVariableName { get; set; }
        public string ProjectDesignSuggestionVariableName { get; set; }
        public string ProjectDesignTemplateName { get; set; }

    }
}
