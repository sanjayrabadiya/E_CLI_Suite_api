using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.LanguageSetup
{
    public class VariableLabelLanguageDto : BaseDto
    {
        public int ProjectDesignVariableId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public IList<VariableLabelLanguageDto> variableLabelLanguages { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public Language Language { get; set; }
    }

    public class VariableLabelLanguageGridDto : BaseAuditDto
    {
        public int ProjectDesignVariableId { get; set; }
        public string Note { get; set; }
        public string LanguageName { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public Language Language { get; set; }
    }
}
