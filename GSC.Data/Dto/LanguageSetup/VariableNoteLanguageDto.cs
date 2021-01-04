using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.LanguageSetup
{
    public class VariableNoteLanguageDto : BaseDto
    {
        public int ProjectDesignVariableId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public IList<VariableNoteLanguageDto> variableNoteLanguages { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public Language Language { get; set; }
    }

    public class VariableNoteLanguageGridDto : BaseAuditDto
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
