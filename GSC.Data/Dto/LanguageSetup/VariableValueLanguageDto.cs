using System.Collections.Generic;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Dto.Master.LanguageSetup
{
    public class VariableValueLanguageDto : BaseDto
    {
        public int ProjectDesignVariableValueId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public IList<VariableValueLanguageDto> VariableValueLanguages { get; set; }
        public ProjectDesignVariableValue ProjectDesignVariableValue { get; set; }
        public Language Language { get; set; }
    }

    public class VariableValueLanguageGridDto : BaseAuditDto
    {
        public int ProjectDesignVariableValueId { get; set; }
        public string ValueName { get; set; }
        public string LanguageName { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public ProjectDesignVariableValue ProjectDesignVariableValue { get; set; }
        public Language Language { get; set; }
    }
}